using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.StaticFiles;

namespace BTC_ReconciliationAutomation.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileRepository _repo;
        private readonly ISystemLogRepository _logRepo;

        public FileController(IFileRepository repo, ISystemLogRepository logRepo)
        {
            _repo = repo;
            _logRepo = logRepo;
        }

        public class DeleteRangeRequest
        {
            public System.DateTime? From { get; set; }
            public System.DateTime? To { get; set; }
            public int? Days { get; set; }
        }

        // Get latest files with related data
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] int count = 50)
        {
            var files = await _repo.GetLatestFilesAsync(count);
            return Ok(files);
        }

        // List files generated in a run
        [HttpGet("run/{runId}")]
        public async Task<IActionResult> ListByRun([FromRoute] object runId)
        {
            var all = await _repo.GetAllAsync();
            var filtered = System.Linq.Enumerable.Where(all, f =>
            {
                var prop = f.GetType().GetProperty("RUN_ID");
                if (prop == null) return false;
                var val = prop.GetValue(f);
                return val != null && val.Equals(runId);
            });
            return Ok(filtered);
        }

        // Download a file (placeholder - assumes file path stored in FILE_PATH property)
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download([FromRoute] object id)
        {
            var file = await _repo.GetByIdAsync(id);
            if (file == null) return NotFound();
            var typeProvider = new FileExtensionContentTypeProvider();
            var pathProp = file.GetType().GetProperty("FILE_PATH") ?? file.GetType().GetProperty("FilePath");
            var path = pathProp?.GetValue(file)?.ToString();
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return NotFound();
            if (!typeProvider.TryGetContentType(path, out var contentType)) contentType = "application/octet-stream";
            var stream = System.IO.File.OpenRead(path);
            return File(stream, contentType, System.IO.Path.GetFileName(path));
        }

        // Get all file types (for filter dropdowns)
        [HttpGet("types")]
        public async Task<IActionResult> GetFileTypes()
        {
            var types = await _repo.GetAllFileTypesAsync();
            return Ok(types.Select(t => new
            {
                id   = t.FILE_TYPE_ID,
                name = t.FILE_TYPE_NAME
            }));
        }

        // Trigger email delivery (placeholder)
        [HttpPost("email/{id}")]
        public async Task<IActionResult> SendEmail([FromRoute] object id)
        {
            // lookup file and queue email/send
            var file = await _repo.GetByIdAsync(id);
            if (file == null) return NotFound();
            // TODO: integrate with email service
            return Accepted(new { message = "Email queued (placeholder)" });
        }

        // Delete file records by date range or number of days
        [HttpPost("delete/range")]
        public async Task<IActionResult> DeleteRange([FromBody] DeleteRangeRequest req)
        {
            var all = await _repo.GetAllAsync();

            IEnumerable<BTC_ReconciliationAutomation.Server.Models.generated_file> filtered;

            if (req?.Days != null && req.Days > 0)
            {
                var cutoff = System.DateTime.UtcNow.AddDays(-req.Days.Value);
                filtered = all.Where(f => f.CREATED_AT < cutoff).ToList();
            }
            else
            {
                var to = req?.To ?? System.DateTime.UtcNow;
                var from = req?.From ?? System.DateTime.MinValue;
                filtered = all.Where(f => f.CREATED_AT >= from && f.CREATED_AT <= to).ToList();
            }

            var count = 0;
            foreach (var f in filtered)
            {
                await _repo.DeleteAsync(f.FILE_ID);
                count++;
            }

            var message = req?.Days != null && req.Days > 0
                ? $"Deleted {count} file record(s) older than {req.Days} days"
                : $"Deleted {count} file record(s) (range: {req?.From:u} - {req?.To:u})";

            var audit = new BTC_ReconciliationAutomation.Server.Models.system_log
            {
                LOG_MESSAGE = message,
                CREATED_AT = System.DateTime.UtcNow,
                LOG_LEVEL_ID = 1
            };
            await _logRepo.AddAsync(audit);

            return Ok(new { deleted = count });
        }

        // Delete all file records
        [HttpPost("delete/all")]
        public async Task<IActionResult> DeleteAll()
        {
            var all = await _repo.GetAllAsync();
            var list = all.ToList();
            var count = 0;
            foreach (var f in list)
            {
                await _repo.DeleteAsync(f.FILE_ID);
                count++;
            }

            var audit = new BTC_ReconciliationAutomation.Server.Models.system_log
            {
                LOG_MESSAGE = $"Deleted all file records ({count} entries)",
                CREATED_AT = System.DateTime.UtcNow,
                LOG_LEVEL_ID = 1
            };
            await _logRepo.AddAsync(audit);

            return Ok(new { deleted = count });
        }
    }
}