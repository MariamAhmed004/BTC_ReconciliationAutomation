using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReconciliationController : ControllerBase
    {
        private readonly IReconciliationRunRepository _repo;
        private readonly IReconciliationStatsRepository _statsRepo;

        public ReconciliationController(IReconciliationRunRepository repo, IReconciliationStatsRepository statsRepo)
        {
            _repo = repo;
            _statsRepo = statsRepo;
        }

        // Trigger a new run (manual)
        [HttpPost("trigger")]
        public async Task<IActionResult> Trigger([FromBody] reconciliation_run run)
        {
            if (run == null) return BadRequest();
            await _repo.AddAsync(run);
            return CreatedAtAction(nameof(Get), new { id = run.RUN_ID }, run);
        }

        // Run the main reconciliation stored procedure
        [HttpPost("run")]
        public async Task<IActionResult> RunReconciliation()
        {
            var result = await _repo.RunMainReconciliationAsync();
            return Ok(new { message = result });
        }

        // Get run status and details
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] object id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        // Get detailed run information with all related data
        [HttpGet("run/{id}")]
        public async Task<IActionResult> GetRunDetails([FromRoute] int id)
        {
            var run = await _repo.GetByIdAsync(id);
            if (run == null) return NotFound(new { message = "Run not found" });

            // Get summary data
            var summary = run.reconciliation_summaries?.FirstOrDefault();

            // Get related files
            var files = run.generated_files?.Select(f => new
            {
                fileId = f.FILE_ID,
                fileName = f.FILE_NAME,
                filePath = f.SERVER_FILE_PATH,
                createdAt = f.CREATED_AT,
                fileType = f.FILE_TYPE?.FILE_TYPE_NAME
            }).ToList();

            // Get related logs
            var logs = run.system_logs?.Select(l => new
            {
                logId = l.LOG_ID,
                message = l.LOG_MESSAGE,
                createdAt = l.CREATED_AT,
                logLevel = l.LOG_LEVEL?.LOG_LEVEL1,
                logLevelId = l.LOG_LEVEL_ID
            }).ToList();

            // Get configuration info
            var config = run.CONFIG;

            return Ok(new
            {
                runId = run.RUN_ID,
                status = run.RUN_STATUS?.RUN_STATUS1,
                runDate = run.RUN_DATE,
                errorMessage = run.ERROR_MESSAGE,
                triggeredBy = run.TRIGGERED_BY,
                deliveryMethod = run.DELIVERY_METHOD?.DELIVERY_METHOD1,
                emailStatus = run.EMAIL_STATUS?.EMAIL_STATUS1,
                // Summary data
                recordsProcessed = summary?.TOTAL_RECORDS_PROCESSED ?? 0,
                totalDiscrepancies = summary?.TOTAL_DISCREPANCIES ?? 0,
                mismatchCount = summary?.MISMATCH_COUNT ?? 0,
                missingInCustomer = summary?.MISSING_IN_CUSTOMER_COUNT ?? 0,
                missingInBilling = summary?.MISSING_IN_BILLING_COUNT ?? 0,
                // Configuration data
                configuration = config != null ? new
                {
                    configId = config.CONFIG_ID,
                    emailRecipients = config.EMAIL_RECIPIENTS,
                    scheduleExpression = $"{config.FREQUENCY} on {config.DAY_OF_MONTH} at {config.RUN_TIME}",
                    isActive = config.IS_ACTIVE,
                    effectiveFrom = config.EFFECTIVE_FROM,
                    effectiveTo = config.EFFECTIVE_TO
                } : null,
                // Related data
                files = files?.Cast<object>().ToList() ?? new List<object>(),
                logs = logs?.Cast<object>().ToList() ?? new List<object>()
            });
        }

        // List past runs (optional status filter)
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? status = null)
        {
            var all = await _repo.GetAllAsync();
            if (!string.IsNullOrEmpty(status))
            {
                // Filter by RUN_STATUS relationship
                var filtered = all.Where(r =>
                    r.RUN_STATUS != null &&
                    string.Equals(r.RUN_STATUS.RUN_STATUS1, status, System.StringComparison.OrdinalIgnoreCase)
                );
                return Ok(filtered);
            }

            return Ok(all);
        }

        // Get dashboard statistics
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var all = await _repo.GetAllAsync();
            var allList = all.ToList();

            // Get last execution
            var lastRun = allList
                .OrderByDescending(r => r.RUN_DATE)
                .FirstOrDefault();

            // Get last execution status from RUN_STATUS relationship
            var lastStatus = lastRun?.RUN_STATUS?.RUN_STATUS1 ?? "N/A";
            var lastRunDate = lastRun?.RUN_DATE;

            // Get the latest summary for discrepancy counts
            var lastSummary = lastRun?.reconciliation_summaries?.FirstOrDefault();

            // Calculate total runs this month
            var currentMonth = System.DateTime.Now.Month;
            var currentYear = System.DateTime.Now.Year;
            var totalRunsThisMonth = allList.Count(r => 
                r.RUN_DATE.Month == currentMonth && 
                r.RUN_DATE.Year == currentYear);

            // Calculate total discrepancies from the last run
            var totalDiscrepancies = lastSummary?.TOTAL_DISCREPANCIES ?? 0;
            var missingInRowb = lastSummary?.MISSING_IN_BILLING_COUNT ?? 0;
            var mismatchedPackages = lastSummary?.MISMATCH_COUNT ?? 0;

            return Ok(new
            {
                lastExecutionStatus = lastStatus,
                lastExecutionDate = lastRunDate,
                lastExecutionError = lastRun?.ERROR_MESSAGE,
                totalRunsThisMonth = totalRunsThisMonth,
                totalDiscrepancies = totalDiscrepancies,
                missingInRowb = missingInRowb,
                mismatchedPackages = mismatchedPackages,
                triggeredBy = lastRun?.TRIGGERED_BY,
                deliveryMethod = lastRun?.DELIVERY_METHOD?.DELIVERY_METHOD1,
                emailStatus = lastRun?.EMAIL_STATUS?.EMAIL_STATUS1
            });
        }

        // Get last execution details
        [HttpGet("dashboard/last-execution")]
        public async Task<IActionResult> GetLastExecution()
        {
            var all = await _repo.GetAllAsync();
            var lastRun = all
                .OrderByDescending(r => r.RUN_DATE)
                .FirstOrDefault();

            if (lastRun == null)
                return NotFound(new { message = "No execution found" });

            return Ok(new
            {
                runId = lastRun.RUN_ID,
                status = lastRun.RUN_STATUS?.RUN_STATUS1,
                runDate = lastRun.RUN_DATE,
                errorMessage = lastRun.ERROR_MESSAGE,
                triggeredBy = lastRun.TRIGGERED_BY,
                deliveryMethod = lastRun.DELIVERY_METHOD?.DELIVERY_METHOD1,
                emailStatus = lastRun.EMAIL_STATUS?.EMAIL_STATUS1
            });
        }

        // Get chart data for dashboard
        [HttpGet("dashboard/charts")]
        public async Task<IActionResult> GetChartData()
        {
            var all = await _repo.GetAllAsync();
            var allList = all.ToList();

            // Get the last run's summary for pie chart
            var lastRun = allList.OrderByDescending(r => r.RUN_DATE).FirstOrDefault();
            var lastSummary = lastRun?.reconciliation_summaries?.FirstOrDefault();

            // Pie chart data - discrepancy types
            var pieChartData = new
            {
                missingInBilling = lastSummary?.MISSING_IN_BILLING_COUNT ?? 0,
                missingInCustomer = lastSummary?.MISSING_IN_CUSTOMER_COUNT ?? 0,
                mismatch = lastSummary?.MISMATCH_COUNT ?? 0
            };

            // Line chart data - total discrepancies over time (last 12 months)
            var oneYearAgo = System.DateTime.Now.AddMonths(-12);
            var runsLastYear = allList
                .Where(r => r.RUN_DATE >= oneYearAgo)
                .OrderBy(r => r.RUN_DATE)
                .ToList();

            var lineChartData = runsLastYear.Select(r => new
            {
                date = r.RUN_DATE,
                totalDiscrepancies = r.reconciliation_summaries?.FirstOrDefault()?.TOTAL_DISCREPANCIES ?? 0
            }).ToList();

            // Success rate calculation using RUN_STATUS relationship
            var totalRuns = allList.Count;
            var completedRuns = allList.Count(r => 
                r.RUN_STATUS != null &&
                string.Equals(r.RUN_STATUS.RUN_STATUS1, "COMPLETED", System.StringComparison.OrdinalIgnoreCase));
            var successRate = totalRuns > 0 ? (decimal)completedRuns / totalRuns * 100 : 0;

            return Ok(new
            {
                pieChart = pieChartData,
                lineChart = lineChartData,
                successRate = System.Math.Round(successRate, 2)
            });
        }

        // Get live reconciliation statistics
        [HttpGet("dashboard/live-stats")]
        public async Task<IActionResult> GetLiveStats()
        {
            try
            {
                // Run queries sequentially to avoid DbContext threading issues
                var missingInRowb = await _statsRepo.GetMissingInRowbCountAsync();
                var notActiveInSiebel = await _statsRepo.GetNotActiveInSiebelCountAsync();
                var mismatchedPackages = await _statsRepo.GetMismatchedPackagesCountAsync();

                return Ok(new
                {
                    missingInRowb = missingInRowb,
                    notActiveInSiebel = notActiveInSiebel,
                    mismatchedPackages = mismatchedPackages,
                    timestamp = System.DateTime.Now
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching live stats", error = ex.Message });
            }
        }
    }
}