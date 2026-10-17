using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Agent.Core.Services;
using Agent.Core.Entities;
using Agent.Core.Interfaces;

namespace Agent.Api
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _mqHost;
        private readonly string _mqUser;
        private readonly string _mqPass;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _mqHost = Environment.GetEnvironmentVariable("MQ_HOST") ?? "localhost";
            _mqUser = Environment.GetEnvironmentVariable("MQ_USER") ?? "agent_mq";
            _mqPass = Environment.GetEnvironmentVariable("MQ_PASS") ?? "SecretMq123!";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory 
            { 
                HostName = _mqHost, 
                UserName = _mqUser, 
                Password = _mqPass 
            };
            
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "invoice.received", 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Guid invoiceId = Guid.Empty;

                try 
                {
                    using var payloadDoc = JsonDocument.Parse(message);
                    invoiceId = payloadDoc.RootElement.GetProperty("InvoiceId").GetGuid();
                    
                    _logger.LogInformation("Procesando factura ID: {InvoiceId}", invoiceId);

                    using var scope = _scopeFactory.CreateScope();
                    var engine = scope.ServiceProvider.GetRequiredService<InvoiceEvaluationEngine>();
                    var repository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();

                    var parsedInvoice = new ParsedInvoice 
                    {
                        VendorId = Guid.NewGuid(),
                        VendorName = "Tech Solutions LLC",
                        Amount = 1500m,
                        PaymentDate = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(15),
                        ServiceMonth = "2026-10"
                    };

                    var rules = new SystemRules { HumanApprovalThreshold = 5000m };

                    var result = await engine.EvaluateInvoiceAsync(parsedInvoice, rules);

                    var reasons = string.Join(" | ", result.Explanations);
                    var color = result.Status switch 
                    {
                        InvoiceStatus.Green => "Verde",
                        InvoiceStatus.Yellow => "Amarillo",
                        InvoiceStatus.Red => "Rojo",
                        _ => "Desconocido"
                    };
                    var finalStatus = result.Status == InvoiceStatus.Green ? "Pagada" : result.Status == InvoiceStatus.Yellow ? "Pendiente" : "Frenada";

                    await repository.UpdateInvoiceResultAsync(invoiceId, finalStatus, color, reasons);

                    _logger.LogWarning("Evaluación Completada. Semáforo: {Color}. Explicación: {Reasons}", color, reasons);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error fatal procesando mensaje RabbitMQ. Factura ID: {InvoiceId}", invoiceId);
                    
                    if(invoiceId != Guid.Empty)
                    {
                        try 
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var repository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                            await repository.UpdateInvoiceResultAsync(invoiceId, "Error", "Amarillo", $"Fallo de sistema: {ex.Message}");
                        }
                        catch { /* Supresión para evitar bucles de error */ }
                    }
                }
            };

            await channel.BasicConsumeAsync(
                queue: "invoice.received", 
                autoAck: true, 
                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}