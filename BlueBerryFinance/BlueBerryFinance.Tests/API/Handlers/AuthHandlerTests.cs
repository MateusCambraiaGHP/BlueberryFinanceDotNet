using BlueBerryFinance.API.Application.Handlers;
using BlueBerryFinance.API.Application.Requests.Auth;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Infrastructure.Models;
using BlueBerryFinance.API.Infrastructure.Services;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace BlueBerryFinance.Tests.API.Handlers
{
    public class AuthHandlerTests
    {
        private static async Task<AppDbContext> CreateDbWithAdmin(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            var admin = new User
            {
                Email = "admin@test.com",
                Name = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPass123!"),
                Profile = "Admin",
                Active = 1
            };
            admin.SetInsertionDate(DateTime.UtcNow);
            admin.SetLastModification(DateTime.UtcNow);
            db.Users.Add(admin);
            await db.SaveChangesAsync();
            return db;
        }

        [Fact]
        public async Task HandleAsync_WithValidCredentials_ReturnsToken()
        {
            var sp = TestServiceFactory.Build();
            var db = await CreateDbWithAdmin(sp);
            var jwtOptions = sp.GetRequiredService<IOptions<JwtOptions>>();
            var jwtService = new JwtService(jwtOptions);
            var handler = new AuthHandler(db, jwtService, jwtOptions);

            var result = await handler.HandleAsync(new LoginRequest
            {
                Email = "admin@test.com",
                Password = "TestPass123!"
            });

            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            result.Profile.Should().Be("Admin");
        }

        [Fact]
        public async Task HandleAsync_WithWrongPassword_ReturnsNull()
        {
            var sp = TestServiceFactory.Build();
            var db = await CreateDbWithAdmin(sp);
            var jwtOptions = sp.GetRequiredService<IOptions<JwtOptions>>();
            var jwtService = new JwtService(jwtOptions);
            var handler = new AuthHandler(db, jwtService, jwtOptions);

            var result = await handler.HandleAsync(new LoginRequest
            {
                Email = "admin@test.com",
                Password = "WrongPassword"
            });

            result.Should().BeNull();
        }

        [Fact]
        public async Task HandleAsync_WithNonExistentUser_ReturnsNull()
        {
            var sp = TestServiceFactory.Build();
            var db = sp.GetRequiredService<AppDbContext>();
            var jwtOptions = sp.GetRequiredService<IOptions<JwtOptions>>();
            var jwtService = new JwtService(jwtOptions);
            var handler = new AuthHandler(db, jwtService, jwtOptions);

            var result = await handler.HandleAsync(new LoginRequest
            {
                Email = "nobody@test.com",
                Password = "AnyPassword"
            });

            result.Should().BeNull();
        }
    }
}
