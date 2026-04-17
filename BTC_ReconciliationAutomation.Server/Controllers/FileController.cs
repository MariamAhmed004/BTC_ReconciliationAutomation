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

        public FileController(IFileRepository repo)
        {
            _repo = repo;
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
    }
}