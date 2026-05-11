using BlueBerryFinance.API.Application.Features.Chat;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    [Route("chat")]
    public class ChatController : BaseController
    {
        private readonly IChatHandler _chatHandler;
        private readonly IMinioService _minio;

        private static readonly string[] _allowedImageTypes =
            ["image/jpeg", "image/png", "image/gif", "image/webp"];

        public ChatController(
            IChatHandler chatHandler,
            IMinioService minio)
        {
            _chatHandler = chatHandler;
            _minio = minio;
        }

        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            if (!_allowedImageTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only image files are accepted (JPEG, PNG, GIF, WebP)." });

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { error = "Image size must not exceed 10 MB." });

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _minio.UploadAsync(stream, file.FileName, file.ContentType);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("stream")]
        public async Task StreamChat([FromBody] ChatStreamRequest request)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            try
            {
                await foreach (var chunk in _chatHandler.StreamAsync(request.Prompt, includeTools: true))
                {
                    var data = JsonSerializer.Serialize(new { content = chunk });
                    await Response.WriteAsync($"data: {data}\n\n");
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                //TODO :: ADD LOGS
                var error = JsonSerializer.Serialize(new { error = "An error occurred during streaming." });
                await Response.WriteAsync($"data: {error}\n\n");
                await Response.Body.FlushAsync();
            }
            finally
            {
                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();
            }
        }
    }
}
