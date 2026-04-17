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

        public ReconciliationController(IReconciliationRunRepository repo)
        {
            _repo = repo;
        }

        // Trigger a new run (manual)
        [HttpPost("trigger")]
        public async Task<IActionResult> Trigger([FromBody] reconciliation_run run)
        {
            if (run == null) return BadRequest();
            await _repo.AddAsync(run);
            return CreatedAtAction(nameof(Get), new { id = run.RUN_ID }, run);
        }

        // Get run status and details
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] object id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        // List past runs (optional status filter)
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? status = null)
        {
            var all = await _repo.GetAllAsync();
            if (!string.IsNullOrEmpty(status))
            {
                // attempt to filter by a property named STATUS (case-insensitive)
                var filtered = System.Linq.Enumerable.Where(all, r =>
                {
                    var type = r.GetType();
                    var prop = type.GetProperty("STATUS") ?? type.GetProperty("Status");
                    if (prop == null) return false;
                    var val = prop.GetValue(r)?.ToString();
                    return string.Equals(val, status, System.StringComparison.OrdinalIgnoreCase);
                });
                return Ok(filtered);
            }

            return Ok(all);
        }
    }
}