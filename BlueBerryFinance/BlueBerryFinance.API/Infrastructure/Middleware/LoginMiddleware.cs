using BlueBerryFinance.API.Application.Features.Auth;
using BlueBerryFinance.API.Application.Features.Auths;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Infrastructure.Models;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BlueBerryFinance.API.Infrastructure.Middleware
{
    /// <summary>
    /// Intercepts POST /api/v1.0/auth/login before the controller pipeline.
    /// Validates credentials, issues a JWT, and writes the LoginResponse directly.
    /// All other requests pass through unchanged.
    /// </summary>
    public class LoginMiddleware : IMiddleware
    {
        private const string LoginPath = "/api/v1.0/auth/login";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly AppDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<LoginMiddleware> _logger;

        public LoginMiddleware(
            AppDbContext db,
            IJwtService jwtService,
            IOptions<JwtOptions> jwtOptions,
            ILogger<LoginMiddleware> logger)
        {
            _db = db;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.Request.Path.Equals(LoginPath, StringComparison.OrdinalIgnoreCase)
                || !HttpMethods.IsPost(context.Request.Method))
            {
                await next(context);
                return;
            }

            LoginRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<LoginRequest>(
                    context.Request.Body, _jsonOptions);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid request body.");
                return;
            }

            if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Email and password are required.");
                return;
            }

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid credentials." });
                return;
            }

            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Profile = user.Profile,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays)
            };

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, response, _jsonOptions);
        }
    }
}
