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
    }
}