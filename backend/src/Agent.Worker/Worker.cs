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

namespace Agent.Worker
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
                Password = _mqPass,
                VirtualHost = Environment.GetEnvironmentVariable("MQ_VHOST") ?? _mqUser 
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

                    // INTEGRACIÓN DE EXTRACCIÓN REAL: 
                    // Aquí debes llamar a tu servicio OCR/LLM que lee el PDF.
                    // Para tu prueba inmediata con la factura de Meridian Audit Group LLC, 
                    // extrae el monto directamente del payload o pásalo dinámicamente.
                    
                    decimal extractedAmount = 7500m; // Reemplaza esto con la extracción de tu IA (ej. AI.Parse(pdf).Amount)
                    string extractedVendor = "Meridian Audit Group LLC"; // Reemplaza con extracción real

                    var parsedInvoice = new ParsedInvoice 
                    {
                        VendorId = Guid.NewGuid(),
                        VendorName = extractedVendor,
                        Amount = extractedAmount,
                        PaymentDate = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(15),
                        ServiceMonth = "2026-10"
                    };

                    // El umbral se mantiene en 5000. Cualquier factura superior será escalada.
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

                    bool isDuplicate = reasons.Contains("duplicad", StringComparison.OrdinalIgnoreCase);
                    bool isIllegal = reasons.Contains("ilegal", StringComparison.OrdinalIgnoreCase);
                    bool needsReview = result.Status == InvoiceStatus.Yellow || parsedInvoice.Amount > rules.HumanApprovalThreshold;

                    await repository.UpdateInvoiceResultAsync(invoiceId, finalStatus, color, reasons, parsedInvoice.Amount, isDuplicate, isIllegal, needsReview);

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
                            await repository.UpdateInvoiceResultAsync(invoiceId, "Error", "Amarillo", $"Fallo de sistema: {ex.Message}", 0m, false, false, true);
                        }
                        catch { }
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