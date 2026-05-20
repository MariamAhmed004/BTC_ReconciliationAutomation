using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.DTOs;

namespace BTC_ReconciliationAutomation.Server.Controllers
{
    [Authorize]
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

        // Create a new active configuration and deactivate the previous active one
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateConfigurationRequest request)
        {
            if (request == null) return BadRequest("Configuration payload is required.");

            var config = new system_configuration
            {
                EMAIL_RECIPIENTS = request.EMAIL_RECIPIENTS,
                RUN_TIME = request.RUN_TIME,
                FREQUENCY = request.FREQUENCY,
                DAY_OF_MONTH = request.DAY_OF_MONTH,
                DAYS_TO_DELETE_AUDITLOGS = request.DAYS_TO_DELETE_AUDITLOGS,
                DEFAULT_FILE_PATH = request.DEFAULT_FILE_PATH,
                IGNORE_CONDITIONS = request.IGNORE_CONDITIONS,
                ADDED_BY = request.ADDED_BY
            };

            var created = await _repo.CreateNewActiveAsync(config);
            return CreatedAtAction(nameof(GetById), new { id = created.CONFIG_ID }, created);
        }

        // Update a configuration value
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] object id, [FromBody] system_configuration config)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.UpdateAsync(config);
            return NoContent();
        }

        // Check whether the Oracle PURGE_LOGS_JOB scheduler job is currently enabled
        [HttpGet("purge-job-status")]
        public async Task<IActionResult> GetPurgeJobStatus()
        {
            var enabled = await _repo.IsPurgeJobEnabledAsync();
            return Ok(new { jobName = "PURGE_LOGS_JOB", enabled });
        }
    }
}