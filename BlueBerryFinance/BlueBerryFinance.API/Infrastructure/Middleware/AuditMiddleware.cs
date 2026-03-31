using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BlueBerryFinance.API.Infrastructure.Middleware
{
    public class AuditMiddleware
    {
        private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "passwordHash", "token", "secret", "apiKey", "key"
        };

        private const int MaxPayloadBytes = 32 * 1024; // 32 KB

        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AuditDbContext auditDb)
        {
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            context.Request.Headers["X-Correlation-Id"] = correlationId;
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            var requestBody = await ReadBodyAsync(context.Request);
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var sw = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();

                var responseBodyText = await ReadResponseBodyAsync(responseBody, originalBodyStream);

                var userId = context.User.FindFirstValue("userId");
                var routeData = context.GetRouteData();
                var controller = routeData?.Values["controller"]?.ToString() ?? string.Empty;
                var action = routeData?.Values["action"]?.ToString() ?? string.Empty;

                var log = new AuditLog
                {
                    CorrelationId = correlationId,
                    UserId = userId is not null ? Guid.TryParse(userId, out var uid) ? uid : null : null,
                    Controller = controller,
                    Action = action,
                    Method = context.Request.Method,
                    Payload = JsonSerializer.Serialize(
                        Truncate(RedactSensitive(requestBody), MaxPayloadBytes)
                        ),
                    Response = JsonSerializer.Serialize(
                        Truncate(RedactSensitive(responseBodyText), MaxPayloadBytes)
                        ),
                    StatusCode = context.Response.StatusCode,
                    DurationMs = sw.ElapsedMilliseconds,
                    InsertionDate = DateTime.UtcNow
                };

                try
                {
                    auditDb.AuditLogs.Add(log);
                    await auditDb.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write audit log for correlation {CorrelationId}", correlationId);
                }
            }
        }

        private static async Task<string> ReadBodyAsync(HttpRequest request)
        {
            if (!request.Body.CanSeek)
            {
                request.EnableBuffering();
            }

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private static async Task<string> ReadResponseBodyAsync(MemoryStream responseBody, Stream originalStream)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalStream);
            return text;
        }

        private static string RedactSensitive(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return json;

            foreach (var field in SensitiveFields)
            {
                // Simple regex-free redaction: replace known key patterns
                json = System.Text.RegularExpressions.Regex.Replace(
                    json,
                    $@"(""{field}""\s*:\s*)""\S+""",
                    $"$1\"[REDACTED]\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return json;
        }

        private static string Truncate(string value, int maxBytes)
        {
            if (Encoding.UTF8.GetByteCount(value) <= maxBytes) return value;
            var bytes = Encoding.UTF8.GetBytes(value);
            return Encoding.UTF8.GetString(bytes, 0, maxBytes) + "...[truncated]";
        }
    }
}
