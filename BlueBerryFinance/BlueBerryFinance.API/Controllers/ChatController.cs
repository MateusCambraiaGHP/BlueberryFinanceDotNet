using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests;
using BlueBerryFinance.API.Application.Requests.Chat;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    public class ChatController : BaseController
    {
        private readonly IChatHandler _chatHandler;
        private readonly IMinioService _minio;
        private readonly IConfiguration _config;
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
            IConfiguration config,
            ILogger<ChatController> logger)
        {
            _chatHandler = chatHandler;
            _minio = minio;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an image to MinIO and returns its public URL.
        /// The URL can then be included in a chat message so the agent can call analyze_image.
        /// </summary>
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

        /// <summary>
        /// OpenAI-compatible streaming completions endpoint consumed by LibreChat.
        /// Authenticated via static service key (Bearer token) — no user JWT required.
        /// Write tools are disabled; read-only conversational responses only.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("/v1/models")]
        [HttpGet("/models")]
        public IActionResult GetModels()
        {
            return Ok(new
            {
                @object = "list",
                data = new[]
                {
                    new { id = "blueberry-financial-assistant", @object = "model", owned_by = "blueberry" }
                }
            });
        }

        [AllowAnonymous]
        [HttpPost("chat/completions")]
        [HttpPost("v1/chat/completions")]
        [HttpPost("/chat/completions")]
        [HttpPost("/v1/chat/completions")]
        public async Task ChatCompletions([FromBody] OpenAIChatCompletionsRequest request, CancellationToken ct)
        {
            if (!ValidateServiceKey())
            {
                Response.StatusCode = 401;
                return;
            }

            // Resolve user: prefer request.User (GUID), fall back to configured default
            Guid userId = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(request.User) && Guid.TryParse(request.User, out var requestUserId))
                userId = requestUserId;
            else
            {
                var defaultId = _config["LibreChat:DefaultUserId"];
                if (!string.IsNullOrWhiteSpace(defaultId))
                    Guid.TryParse(defaultId, out userId);
            }

            if (userId == Guid.Empty)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("LibreChat:DefaultUserId is not configured.");
                return;
            }

            // Inject userId as a claim so tools can resolve the current user
            var identity = new ClaimsIdentity("LibreChat");
            identity.AddClaim(new Claim("userId", userId.ToString()));
            HttpContext.User = new ClaimsPrincipal(identity);

            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            var prompt = request.Messages
                .LastOrDefault(m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase))
                ?.Content ?? string.Empty;

            var completionId = $"chatcmpl-{Guid.NewGuid():N}";
            var created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                await foreach (var chunk in _chatHandler.StreamAsync(prompt, includeTools: true, ct))
                {
                    var data = JsonSerializer.Serialize(new
                    {
                        id = completionId,
                        @object = "chat.completion.chunk",
                        created,
                        model = request.Model,
                        choices = new[]
                        {
                            new
                            {
                                index = 0,
                                delta = new { content = chunk },
                                finish_reason = (string?)null
                            }
                        }
                    }, _jsonOptions);

                    await Response.WriteAsync($"data: {data}\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                }

                // Final chunk
                var done = JsonSerializer.Serialize(new
                {
                    id = completionId,
                    @object = "chat.completion.chunk",
                    created,
                    model = request.Model,
                    choices = new[]
                    {
                        new
                        {
                            index = 0,
                            delta = new { },
                            finish_reason = "stop"
                        }
                    }
                }, _jsonOptions);

                await Response.WriteAsync($"data: {done}\n\n", ct);
                await Response.WriteAsync("data: [DONE]\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during LibreChat completions stream");
            }
        }

        private bool ValidateServiceKey()
        {
            var serviceKey = _config["LibreChat:ServiceApiKey"];
            if (string.IsNullOrWhiteSpace(serviceKey)) return false;

            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            return authHeader == $"Bearer {serviceKey}";
        }
    }
}
