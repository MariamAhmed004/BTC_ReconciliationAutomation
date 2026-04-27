using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ISystemLogRepository _repo;

        public LogController(ISystemLogRepository repo)
        {
            _repo = repo;
        }

        public class DeleteRangeRequest
        {
            public System.DateTime? From { get; set; }
            public System.DateTime? To { get; set; }
            public int? Days { get; set; }
        }

        // Get system logs for a run
        [HttpGet("run/{runId}")]
        public async Task<IActionResult> GetForRun([FromRoute] object runId)
        {
            var all = await _repo.GetAllAsync();
            var filtered = System.Linq.Enumerable.Where(all, l =>
            {
                var prop = l.GetType().GetProperty("RUN_ID");
                if (prop == null) return false;
                var val = prop.GetValue(l);
                return val != null && val.Equals(runId);
            });
            return Ok(filtered);
        }

        // Get latest logs (for monitoring)
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var all = await _repo.GetAllAsync();
            var latest = System.Linq.Enumerable.Take(all, 50);
            return Ok(latest);
        }

        // Delete logs by date range or number of days
        [HttpPost("delete/range")]
        public async Task<IActionResult> DeleteRange([FromBody] DeleteRangeRequest req)
        {
            var all = await _repo.GetAllAsync();

            var to = req?.To ?? System.DateTime.UtcNow;
            var from = req?.From ?? System.DateTime.MinValue;
            if (req?.Days != null && req.Days > 0)
            {
                from = System.DateTime.UtcNow.AddDays(-req.Days.Value);
            }

            var filtered = System.Linq.Enumerable.Where(all, l => l.CREATED_AT >= from && l.CREATED_AT <= to).ToList();
            var count = 0;
            foreach (var f in filtered)
            {
                await _repo.DeleteAsync(f.LOG_ID);
                count++;
            }

            // record deletion action in logs (LOG_LEVEL_ID set to 1 by default)
            var audit = new BTC_ReconciliationAutomation.Server.Models.system_log
            {
                LOG_MESSAGE = $"Deleted {count} log records (range: {from:u} - {to:u})",
                CREATED_AT = System.DateTime.UtcNow,
                LOG_LEVEL_ID = 1
            };
            await _repo.AddAsync(audit);

            return Ok(new { deleted = count });
        }

        // Delete all logs
        [HttpPost("delete/all")]
        public async Task<IActionResult> DeleteAll()
        {
            var all = await _repo.GetAllAsync();
            var list = System.Linq.Enumerable.ToList(all);
            var count = 0;
            foreach (var l in list)
            {
                await _repo.DeleteAsync(l.LOG_ID);
                count++;
            }

            var audit = new BTC_ReconciliationAutomation.Server.Models.system_log
            {
                LOG_MESSAGE = $"Deleted all log records ({count} entries)",
                CREATED_AT = System.DateTime.UtcNow,
                LOG_LEVEL_ID = 1
            };
            await _repo.AddAsync(audit);

            return Ok(new { deleted = count });
        }
    }
}