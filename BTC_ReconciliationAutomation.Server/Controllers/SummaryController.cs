using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummaryController : ControllerBase
    {
        private readonly ISummaryRepository _repo;

        public SummaryController(ISummaryRepository repo)
        {
            _repo = repo;
        }

        // Get summary for a specific run
        [HttpGet("run/{runId}")]
        public async Task<IActionResult> GetForRun([FromRoute] object runId)
        {
            var s = await _repo.GetByIdAsync(runId);
            if (s == null) return NotFound();
            return Ok(s);
        }

        // Get aggregated stats (basic example)
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var all = await _repo.GetAllAsync();
            // placeholder aggregated example
            var count = System.Linq.Enumerable.Count(all);
            return Ok(new { totalSummaries = count });
        }
    }
}