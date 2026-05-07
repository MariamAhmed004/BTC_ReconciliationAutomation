using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiSummaryController : ControllerBase
    {
        public sealed record AiSummaryDebug(
            int? Status = null,
            string? Body = null,
            string? Error = null,
            string? Url = null,
            string? Server = null);
        public sealed record AiSummaryResponse(string[] Sentences, AiSummaryDebug? Debug = null);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiSummaryController> _logger;
        private readonly IReconciliationRunRepository _runRepo;

        private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

        public AiSummaryController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AiSummaryController> logger,
            IReconciliationRunRepository runRepo)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _runRepo = runRepo;
        }

        [HttpGet("summary/{runId}")]
        public async Task<ActionResult<AiSummaryResponse>> GetSummary([FromRoute] int runId)
        {
            var apiKey = _configuration["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Groq ApiKey is not configured.");
                return Ok(new AiSummaryResponse(
                    Sentences: ["AI summary could not be generated."],
                    Debug: new AiSummaryDebug(Error: "API key not configured.")
                ));
            }

            var run = await _runRepo.GetByIdAsync(runId);
            if (run == null)
                return NotFound(new AiSummaryResponse(Sentences: [$"Run ID {runId} not found."]));

            var summary       = run.reconciliation_summaries?.FirstOrDefault();
            int totalRecords       = (int)(summary?.TOTAL_RECORDS_PROCESSED  ?? 0);
            int totalDiscrepancies = (int)(summary?.TOTAL_DISCREPANCIES      ?? 0);
            int missingInCustomer  = (int)(summary?.MISSING_IN_CUSTOMER_COUNT ?? 0);
            int missingInBilling   = (int)(summary?.MISSING_IN_BILLING_COUNT  ?? 0);
            int mismatchedPackages = (int)(summary?.MISMATCH_COUNT            ?? 0);
            int filesGenerated     = run.generated_files?.Count() ?? 0;
            string runStatus       = run.RUN_STATUS?.RUN_STATUS1 ?? "Unknown";
            string triggeredBy     = run.TRIGGERED_BY ?? "Unknown";
            string runDate         = run.RUN_DATE.ToString("yyyy-MM-dd HH:mm");
            string errorNote       = string.IsNullOrWhiteSpace(run.ERROR_MESSAGE)
                                        ? "No errors occurred."
                                        : $"Error encountered: {run.ERROR_MESSAGE}";

            const string systemMessage =
                "You are a precise business report writer for a financial reconciliation system. " +
                "Your response must contain EXACTLY 3 sentences, each on its own line, with no blank lines between them. " +
                "Do NOT include any preamble, reasoning, numbering, bullet points, or meta-commentary. " +
                "Output only the 3 sentences — nothing else before or after them.";

            var userMessage =
                $"Write exactly 3 formal sentences summarizing this BTC reconciliation run for a business dashboard.\n\n" +
                $"Run ID: {runId}\n" +
                $"Run Date: {runDate}\n" +
                $"Status: {runStatus}\n" +
                $"Triggered By: {triggeredBy}\n" +
                $"Records Processed: {totalRecords:N0}\n" +
                $"Total Discrepancies: {totalDiscrepancies:N0} " +
                $"({missingInCustomer:N0} missing in customer, {missingInBilling:N0} missing in billing, {mismatchedPackages:N0} mismatched packages)\n" +
                $"Files Generated: {filesGenerated}\n" +
                $"{errorNote}\n\n" +
                $"Rules: mention Run ID {runId} for traceability, keep tone professional, output exactly 3 sentences each on its own line.";

            _logger.LogInformation("AI summary requested for Run ID {RunId}", runId);

            try
            {
                var client = _httpClientFactory.CreateClient("Groq");

                var body = JsonSerializer.Serialize(new
                {
                    model = "llama-3.3-70b-versatile",
                    temperature = 0,
                    messages = new[]
                    {
                        new { role = "system",  content = systemMessage },
                        new { role = "user",    content = userMessage   }
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, GroqUrl);
                request.Version = HttpVersion.Version11;
                request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to: {Url}", request.RequestUri);

                HttpResponseMessage response = await client.SendAsync(request);

                var responseBody   = await response.Content.ReadAsStringAsync();
                var responseServer = response.Headers.Server?.ToString();
                _logger.LogInformation("Groq status: {Status} | body: {Body}", response.StatusCode, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Groq returned {Status}: {Body}", response.StatusCode, responseBody);
                    return Ok(new AiSummaryResponse(
                        Sentences: ["AI summary could not be generated."],
                        Debug: new AiSummaryDebug(
                            Status: (int)response.StatusCode,
                            Body: responseBody,
                            Url: request.RequestUri?.ToString(),
                            Server: responseServer)
                    ));
                }

                // Chat completions: choices[0].message.content
                using var document = JsonDocument.Parse(responseBody);
                string? rawText = null;

                if (document.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0 &&
                    choices[0].TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var contentProp))
                {
                    rawText = contentProp.GetString();
                }

                _logger.LogInformation("Extracted raw text: {Text}", rawText);

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    return Ok(new AiSummaryResponse(
                        Sentences: ["AI summary could not be generated."],
                        Debug: new AiSummaryDebug(
                            Status: (int)response.StatusCode,
                            Body: responseBody,
                            Url: request.RequestUri?.ToString(),
                            Server: responseServer)
                    ));
                }

                // Split into individual sentences, drop any blank/reasoning lines
                var sentences = rawText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(s => s.EndsWith('.') || s.EndsWith('!') || s.EndsWith('?'))
                    .Select(s => s.TrimStart('-', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ' ').Trim())
                    .Where(s => s.Length > 0)
                    .ToArray();

                if (sentences.Length == 0)
                {
                    return Ok(new AiSummaryResponse(
                        Sentences: ["AI summary could not be generated."],
                        Debug: new AiSummaryDebug(Body: rawText)
                    ));
                }

                return Ok(new AiSummaryResponse(Sentences: sentences));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling Groq API for Run ID {RunId}", runId);
                return Ok(new AiSummaryResponse(
                    Sentences: ["AI summary could not be generated."],
                    Debug: new AiSummaryDebug(Error: ex.Message)
                ));
            }
        }

        // Intentionally no helper method here: request construction and logging are kept inline
        // to ensure request.RequestUri is logged exactly before sending.
    }
}