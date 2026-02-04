using BlueBerryFinance.API.Application.Handlers;
using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.GeneralPurpose;
using BlueBerryFinance.API.Infrastructure.Utils.Factories;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 


// Configuration
builder.Services.Configure<AIAgentOptions>(builder.Configuration.GetSection("OpenAI"));

// Infrastructure
builder.Services.AddSingleton<IPromptLoader, PromptLoader>();
builder.Services.AddSingleton<IAIAgentFactory, AIAgentFactory>();

// Agents
builder.Services.AddScoped<IFinantialAssistantAgent, FinantialAssistantAgent>();

//Services
builder.Services.AddScoped<ITransactionsHandler, TransactionsHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
