using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;
using BTC_ReconciliationAutomation.Server.DTOs;

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
            var filtered = all.Where(l =>
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
            var latest = all.Take(50);
            return Ok(latest);
        }

        // Delete logs by date range or number of days
        [HttpPost("delete/range")]
        public async Task<IActionResult> DeleteRange([FromBody] DeleteRangeRequest req)
        {
            var all = await _repo.GetAllAsync();

            IEnumerable<system_log> filtered;

            if (req?.Days != null && req.Days > 0)
            {
                // Delete logs older than the specified number of days
                var cutoff = DateTime.UtcNow.AddDays(-req.Days.Value);
                filtered = all.Where(l => l.CREATED_AT < cutoff).ToList();
            }
            else
            {
                var to = req?.To ?? DateTime.UtcNow;
                var from = req?.From ?? DateTime.MinValue;
                filtered = all.Where(l => l.CREATED_AT >= from && l.CREATED_AT <= to).ToList();
            }
            var count = 0;
            foreach (var f in filtered)
            {
                await _repo.DeleteAsync(f.LOG_ID);
                count++;
            }

            // record deletion action in logs (LOG_LEVEL_ID set to 1 by default)
            var message = req?.Days != null && req.Days > 0
                ? $"Deleted {count} log records older than {req.Days} days"
                : $"Deleted {count} log records (range: {req?.From:u} - {req?.To:u})";

            var audit = new system_log
            {
                LOG_MESSAGE = message,
                CREATED_AT = DateTime.UtcNow,
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
            var list = all.ToList();
            var count = 0;
            foreach (var l in list)
            {
                await _repo.DeleteAsync(l.LOG_ID);
                count++;
            }

            var audit2 = new system_log
            {
                LOG_MESSAGE = $"Deleted all log records ({count} entries)",
                CREATED_AT = DateTime.UtcNow,
                LOG_LEVEL_ID = 1
            };
            await _repo.AddAsync(audit2);

            return Ok(new { deleted = count });
        }
    }
}