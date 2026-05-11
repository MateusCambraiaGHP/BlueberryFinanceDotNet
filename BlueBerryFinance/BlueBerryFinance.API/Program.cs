using BlueBerryFinance.API.Application.Handlers;
using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Infrastructure.Jobs;
using BlueBerryFinance.API.Infrastructure.Messaging;
using BlueBerryFinance.API.Infrastructure.Messaging.Consumers;
using BlueBerryFinance.API.Infrastructure.Middleware;
using BlueBerryFinance.API.Infrastructure.Models;
using BlueBerryFinance.API.Infrastructure.Services;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;

// ── Serilog bootstrap ──────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog full config ────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console();

        var seqUrl = ctx.Configuration["Seq:ServerUrl"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            config.WriteTo.Seq(seqUrl, apiKey: ctx.Configuration["Seq:ApiKey"]);
        }
    });

    // ── Configuration bindings ─────────────────────────────────────────────────
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
    builder.Services.Configure<AIAgentOptions>(builder.Configuration.GetSection("LiteLLM"));
    builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
    // ── EF Core — main DB ──────────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

    // ── EF Core — audit DB ────────────────────────────────────────────────────
    builder.Services.AddDbContext<AuditDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Audit")));

    // ── JWT authentication ─────────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["Jwt:Secret"] ?? string.Empty;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

    // ── Authorization policies ─────────────────────────────────────────────────
    builder.Services.AddAuthorization(opt =>
    {
        opt.AddPolicy("AdminOnly", p => p.RequireClaim("profile", "Admin"));
        opt.AddPolicy("UserOrAdmin", p => p.RequireClaim("profile", "Admin", "User"));
    });

    // ── Controllers + Swagger ──────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Security headers ───────────────────────────────────────────────────────
    builder.Services.AddHsts(opt =>
    {
        opt.MaxAge = TimeSpan.FromDays(365);
        opt.IncludeSubDomains = true;
    });

    // ── Infrastructure ─────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IPromptLoader, PromptLoader>();
    builder.Services.AddSingleton<IAIAgentFactory, AIAgentFactory>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));
    builder.Services.AddScoped<IMinioService, MinioService>();

    // ── DB Query Tool (internal — used by SecurityValidationAgent + FinancialAnalysisTool) ──
    builder.Services.AddScoped<IAgentReadTools, AgentReadTools>();
    // ── DB Save Tool (internal — legacy tools, payload records shared with AgentApprovalHandler) ──
    builder.Services.AddScoped<IAgentWriteTools, AgentWriteTools>();

    // ── Sub-agents ────────────────────────────────────────────────────────────
    builder.Services.AddScoped<IFiscalNoteAgent, FiscalNoteAgent>();
    builder.Services.AddScoped<IClassificationAgent, ClassificationAgent>();
    builder.Services.AddScoped<ISecurityValidationAgent, SecurityValidationAgent>();

    // ── Orchestrator tools ────────────────────────────────────────────────────
    builder.Services.AddScoped<IIncomeExpenseSaverTool, IncomeExpenseSaverTool>();
    builder.Services.AddScoped<IExtractProcessorTool, ExtractProcessorTool>();
    builder.Services.AddScoped<IImageAnalyzerTool, ImageAnalyzerTool>();
    builder.Services.AddScoped<IFinancialAnalysisTool, FinancialAnalysisTool>();
    builder.Services.AddScoped<IOrchestratorTools, OrchestratorTools>();

    // ── Blueberry Finance Agent (orchestrator — used for direct structured calls) ──
    builder.Services.AddScoped<IBlueberryFinanceAgent, BlueberryFinanceAgent>();

    // ── Application handlers ───────────────────────────────────────────────────
    builder.Services.AddScoped<IAgentApprovalHandler, AgentApprovalHandler>();
    builder.Services.AddScoped<IChatHandler, ChatHandler>();
    builder.Services.AddScoped<ITransactionHandler, TransactionHandler>();
    builder.Services.AddScoped<IBankAccountHandler, BankAccountHandler>();
    builder.Services.AddScoped<ICategoryHandler, CategoryHandler>();
    builder.Services.AddScoped<IStoreHandler, StoreHandler>();
    builder.Services.AddScoped<IFixedExpenseHandler, FixedExpenseHandler>();
    builder.Services.AddScoped<ICurrencyHandler, CurrencyHandler>();
    builder.Services.AddScoped<IReportHandler, ReportHandler>();
    builder.Services.AddScoped<ICsvImportHandler, CsvImportHandler>();

    // ── Messaging ─────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

    // ── Background jobs & consumers ───────────────────────────────────────────
    builder.Services.AddHostedService<MonthlyReportJob>();
    builder.Services.AddHostedService<PendingApprovalReminderJob>();
    builder.Services.AddHostedService<FiscalNoteConsumer>();

    // ── HttpContextAccessor (needed for correlation ID forwarding) ────────────
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // ── Auto-migrate & seed on startup ────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            db.Database.Migrate();
            auditDb.Database.Migrate();
            await SeedAsync(db, app.Configuration, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration/seed failed.");
        }
    }

    // ── Middleware pipeline ────────────────────────────────────────────────────
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        await next();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    // Audit middleware runs after auth (so we have userId)
    app.UseMiddleware<AuditMiddleware>();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// ── Seed: admin user + currencies ─────────────────────────────────────────────
static async Task SeedAsync(AppDbContext db, IConfiguration config, Microsoft.Extensions.Logging.ILogger logger)
{
    // Seed currencies
    if (!db.Currencies.Any())
    {
        db.Currencies.AddRange(
            new Currency { Code = BlueBerryFinance.API.Data.Entities.Enums.CurrencyCode.BRL, Symbol = "R$", Name = "Brazilian Real", Active = 1 },
            new Currency { Code = BlueBerryFinance.API.Data.Entities.Enums.CurrencyCode.EUR, Symbol = "€", Name = "Euro", Active = 1 }
        );
        foreach (var c in db.Currencies.Local)
        {
            c.SetInsertionDate(DateTime.UtcNow);
            c.SetLastModification(DateTime.UtcNow);
        }
    }

    // Seed admin user
    var adminEmail = config["DEFAULT_ADMIN_EMAIL"] ?? "mateus@hotmail.com";
    var adminPassword = config["DEFAULT_ADMIN_PASSWORD"] ?? "ChangeMe123!";

    if (!db.Users.IgnoreQueryFilters().Any(u => u.Email == adminEmail))
    {
        var admin = new User
        {
            Email = adminEmail,
            Name = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Profile = "Admin",
            Active = 1
        };
        admin.SetInsertionDate(DateTime.UtcNow);
        admin.SetLastModification(DateTime.UtcNow);
        db.Users.Add(admin);
    }

    await db.SaveChangesAsync();
    logger.LogInformation("Database seeded successfully.");
}
