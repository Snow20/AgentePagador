using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using Agent.Core.Interfaces;
using Agent.Infrastructure.Data;
using Agent.Infrastructure.Repositories;
using Agent.Api;
using Agent.Core; 
using Agent.Core.Services; 

var builder = WebApplication.CreateBuilder(args);

// Configuración adaptativa de puertos (Local: 5000 | Render: variable PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddControllers();

// Configuración dinámica de CORS (Local: 4200 | Cloudflare: variable FRONTEND_URL)
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuración de PostgreSQL
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
    ?? "Host=localhost;Database=AgentDb;Username=agent_admin;Password=SecretPassword123!";
builder.Services.AddDbContext<AgentDbContext>(options =>
    options.UseNpgsql(dbConnectionString));

// Configuración de RabbitMQ
var mqHost = Environment.GetEnvironmentVariable("MQ_HOST") ?? "localhost";
var mqUser = Environment.GetEnvironmentVariable("MQ_USER") ?? "agent_mq";
var mqPass = Environment.GetEnvironmentVariable("MQ_PASS") ?? "SecretMq123!";

// Inyección de dependencias de Infraestructura
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IMessagePublisher>(sp => new RabbitMqPublisher(mqHost, mqUser, mqPass));
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<InvoiceEvaluationEngine>();

// Inyectar el Worker integrado
builder.Services.AddHostedService<Worker>();

// Integración de OpenTelemetry a Dynatrace
var dynatraceEndpoint = builder.Configuration["Dynatrace:OtlpEndpoint"] ?? "http://localhost:4317";
var dynatraceToken = builder.Configuration["Dynatrace:ApiToken"] ?? "dummy-token";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Agent.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(dynatraceEndpoint);
            options.Headers = $"Authorization=Api-Token {dynatraceToken}";
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(dynatraceEndpoint);
            options.Headers = $"Authorization=Api-Token {dynatraceToken}";
        }));

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Autoejecución de migraciones (Evitar comandos manuales en el entorno PaaS)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
    db.Database.EnsureCreated();
}

app.Run();