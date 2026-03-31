using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests.Auth;
using BlueBerryFinance.API.Application.Responses.Auth;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Infrastructure.Models;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class AuthHandler : IAuthHandler
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly JwtOptions _jwtOptions;

        public AuthHandler(AppDbContext db, IJwtService jwtService, IOptions<JwtOptions> jwtOptions)
        {
            _db = db;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<LoginResponse?> HandleAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, ct);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var token = _jwtService.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Profile = user.Profile,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays)
            };
        }
    }
}
