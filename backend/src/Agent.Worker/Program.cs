using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using Agent.Infrastructure.Data;
using Agent.Infrastructure.Repositories;
using Agent.Core.Interfaces;
using Agent.Core.Services;
using Agent.Worker;

var builder = Host.CreateApplicationBuilder(args);

var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
    ?? "Host=localhost;Database=AgentDb;Username=agent_admin;Password=SecretPassword123!";

builder.Services.AddDbContext<AgentDbContext>(options =>
    options.UseNpgsql(dbConnectionString));

// Registro estricto de dependencias del motor de reglas
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<InvoiceEvaluationEngine>();

var dynatraceEndpoint = builder.Configuration["Dynatrace:OtlpEndpoint"] ?? "http://localhost:4317";
var dynatraceToken = builder.Configuration["Dynatrace:ApiToken"] ?? "dummy-token";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Agent.Worker"))
    .WithTracing(tracing => tracing
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(dynatraceEndpoint);
            options.Headers = $"Authorization=Api-Token {dynatraceToken}";
        }))
    .WithMetrics(metrics => metrics
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(dynatraceEndpoint);
            options.Headers = $"Authorization=Api-Token {dynatraceToken}";
        }));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();