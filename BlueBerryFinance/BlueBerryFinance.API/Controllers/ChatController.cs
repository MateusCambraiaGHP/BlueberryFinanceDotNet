using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    public class ChatController : BaseController
    {
        private readonly IChatHandler _chatHandler;
        private readonly IMinioService _minio;
        private readonly ILogger<ChatController> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private static readonly string[] _allowedImageTypes =
            ["image/jpeg", "image/png", "image/gif", "image/webp"];

        public ChatController(
            IChatHandler chatHandler,
            IMinioService minio,
            ILogger<ChatController> logger)
        {
            _chatHandler = chatHandler;
            _minio = minio;
            _logger = logger;
        }

        [HttpPost("chat/upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken ct)
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
                var url = await _minio.UploadAsync(stream, file.FileName, file.ContentType, ct);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost("chat/stream")]
        public async Task StreamChat([FromBody] FinancialAnalysisRequest request, CancellationToken ct)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            try
            {
                await foreach (var chunk in _chatHandler.StreamAsync(request.Prompt, includeTools: true, ct))
                {
                    var data = JsonSerializer.Serialize(new { content = chunk });
                    await Response.WriteAsync($"data: {data}\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during chat stream for user {UserId}", CurrentUserId);
                var error = JsonSerializer.Serialize(new { error = "An error occurred during streaming." });
                await Response.WriteAsync($"data: {error}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
            finally
            {
                if (!ct.IsCancellationRequested)
                {
                    await Response.WriteAsync("data: [DONE]\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
        }
    }
}
