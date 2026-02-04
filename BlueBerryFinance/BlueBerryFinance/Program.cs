using BlueBerryFinance.API.Infrastructure.Utils.Agents.Factories;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.GeneralPurpose;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Helpers;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Helpers.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuration
builder.Services.Configure<AIAgentOptions>(builder.Configuration.GetSection("OpenAI"));

// Infrastructure
builder.Services.AddSingleton<IPromptLoader, PromptLoader>();
builder.Services.AddSingleton<IAIAgentFactory, AIAgentFactory>();

// Agents
builder.Services.AddScoped<IFinantialAssistantAgent, FinantialAssistantAgent>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
