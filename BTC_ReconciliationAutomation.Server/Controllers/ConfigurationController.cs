using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationRepository _repo;

        public ConfigurationController(IConfigurationRepository repo)
        {
            _repo = repo;
        }

        // Get all configurations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var all = await _repo.GetAllAsync();
            return Ok(all);
        }

        // Get a single configuration by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var config = await _repo.GetByIdAsync(id);
            if (config == null) return NotFound();
            return Ok(config);
        }

        // Update a configuration value
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] object id, [FromBody] system_configuration config)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            // simple replace - in future map fields
            await _repo.UpdateAsync(config);
            return NoContent();
        }
    }
}