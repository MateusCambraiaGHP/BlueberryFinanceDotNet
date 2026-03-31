using BlueBerryFinance.API.Application.Handlers;
using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Infrastructure.Models;
using BlueBerryFinance.API.Infrastructure.Services;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using BlueBerryFinance.Tests.Data.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlueBerryFinance.Tests.Data
{
    public static class TestServiceFactory
    {
        public static IServiceProvider Build(Action<IServiceCollection>? overrides = null)
        {
            var services = new ServiceCollection();

            // In-memory EF — unique DB per test
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddDbContext<AuditDbContext>(opt =>
                opt.UseInMemoryDatabase(Guid.NewGuid().ToString() + "_audit"));

            // JWT
            services.Configure<JwtOptions>(opt =>
            {
                opt.Secret = "test-secret-key-at-least-32-chars-long!";
                opt.Issuer = "test-issuer";
                opt.Audience = "test-audience";
                opt.ExpiryDays = 1;
            });

            // Services
            services.AddScoped<IJwtService, JwtService>();

            // Handlers
            services.AddScoped<IAuthHandler, AuthHandler>();
            services.AddScoped<ITransactionHandler, TransactionHandler>();
            services.AddScoped<IBankAccountHandler, BankAccountHandler>();
            services.AddScoped<ICategoryHandler, CategoryHandler>();
            services.AddScoped<IStoreHandler, StoreHandler>();
            services.AddScoped<IFixedExpenseHandler, FixedExpenseHandler>();

            // Mock agents
            services.AddScoped<BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces.IFinantialAssistantAgent,
                MockFinancialAssistantAgent>();

            overrides?.Invoke(services);

            return services.BuildServiceProvider();
        }

        public static AppDbContext GetDb(IServiceProvider sp)
            => sp.GetRequiredService<AppDbContext>();
    }
}
