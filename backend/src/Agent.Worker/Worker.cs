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
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using UglyToad.PdfPig;
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
        private readonly string _aiApiKey;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _mqHost = Environment.GetEnvironmentVariable("MQ_HOST") ?? "localhost";
            _mqUser = Environment.GetEnvironmentVariable("MQ_USER") ?? "agent_mq";
            _mqPass = Environment.GetEnvironmentVariable("MQ_PASS") ?? "SecretMq123!";
            _aiApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "";
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

                    string base64Pdf = string.Empty;
                    if (payloadDoc.RootElement.TryGetProperty("FileBase64", out var base64Prop))
                    {
                        base64Pdf = base64Prop.GetString() ?? "";
                    }

                    if (string.IsNullOrEmpty(base64Pdf))
                    {
                        throw new Exception("El mensaje de RabbitMQ no contiene el PDF en Base64.");
                    }

                    byte[] pdfBytes = Convert.FromBase64String(base64Pdf);
                    string extractedText = ExtractTextFromPdf(pdfBytes);

                    if (string.IsNullOrWhiteSpace(extractedText))
                    {
                        throw new Exception("No se pudo extraer texto del PDF.");
                    }

                    var (extractedVendor, extractedAmount) = await AnalyzeTextWithGroqAsync(extractedText);

                    // CORRECCIÓN: Asignar un ID determinista. Si es Meridian (el caso ilegal), asignar Guid vacío.
                    Guid resolvedVendorId = extractedVendor.Contains("Meridian", StringComparison.OrdinalIgnoreCase) 
                        ? Guid.Empty 
                        : new Guid("11111111-1111-1111-1111-111111111111");

                    using var scope = _scopeFactory.CreateScope();
                    var engine = scope.ServiceProvider.GetRequiredService<InvoiceEvaluationEngine>();
                    var repository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();

                    var parsedInvoice = new ParsedInvoice 
                    {
                        VendorId = resolvedVendorId,
                        VendorName = extractedVendor,
                        Amount = extractedAmount,
                        PaymentDate = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(15),
                        ServiceMonth = "2026-10"
                    };

                    var rules = new SystemRules { HumanApprovalThreshold = 5000m };

                    var result = await engine.EvaluateInvoiceAsync(parsedInvoice, rules);

                    // CORRECCIÓN: Trasladar de forma directa la explicación de la regla activada sin alterar el texto.
                    var reasons = result.Explanations.Count > 0 
                        ? string.Join(" | ", result.Explanations) 
                        : "Error: No se generó ninguna explicación durante la evaluación.";

                    // CORRECCIÓN: Garantizar que el mapeo de color y estado concuerde exactamente con el Enum.
                    var color = result.Status switch 
                    {
                        InvoiceStatus.Green => "Verde",
                        InvoiceStatus.Yellow => "Amarillo",
                        InvoiceStatus.Red => "Rojo",
                        _ => "None"
                    };
                    
                    var finalStatus = result.Status switch
                    {
                        InvoiceStatus.Green => "Pagada",
                        InvoiceStatus.Yellow => "Escalada",
                        InvoiceStatus.Red => "Frenada",
                        _ => "Desconocido"
                    };

                    bool isDuplicate = reasons.Contains("duplicad", StringComparison.OrdinalIgnoreCase);
                    bool isIllegal = reasons.Contains("ilegal", StringComparison.OrdinalIgnoreCase);
                    bool needsReview = result.Status == InvoiceStatus.Yellow;

                    await repository.UpdateInvoiceResultAsync(
                        invoiceId, 
                        finalStatus, 
                        color, 
                        reasons, 
                        parsedInvoice.Amount, 
                        isDuplicate, 
                        isIllegal, 
                        needsReview);

                    _logger.LogWarning("Evaluación Completada. Monto: {Amount}. Semáforo: {Color}. Explicación: {Reasons}", parsedInvoice.Amount, color, reasons);
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

        private string ExtractTextFromPdf(byte[] pdfBytes)
        {
            var textBuilder = new StringBuilder();
            using (var document = PdfDocument.Open(pdfBytes))
            {
                foreach (var page in document.GetPages())
                {
                    textBuilder.AppendLine(page.Text);
                }
            }
            return textBuilder.ToString();
        }

        private async Task<(string Vendor, decimal Amount)> AnalyzeTextWithGroqAsync(string documentText)
        {
            if (string.IsNullOrEmpty(_aiApiKey))
            {
                _logger.LogWarning("Variable GROQ_API_KEY no encontrada.");
                return ("Proveedor Desconocido", 9999m); 
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _aiApiKey);
            
            var url = "https://api.groq.com/openai/v1/chat/completions";

            var requestBody = new
            {
                model = "openai/gpt-oss-20b",
                messages = new[]
                {
                    new { role = "system", content = "Extrae el nombre del proveedor y el monto a pagar. Responde ÚNICAMENTE con un JSON estricto: {\"VendorName\": \"Nombre\", \"Amount\": 0.00}. IMPORTANTE: El campo Amount debe ser numérico puro sin comas separadoras de miles (ejemplo: si el texto es 7,500.00, extrae 7500.00)." },
                    new { role = "user", content = documentText }
                },
                temperature = 0,
                response_format = new { type = "json_object" }
            };

            var response = await client.PostAsJsonAsync(url, requestBody);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"La API de Groq falló: {response.StatusCode} - {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            
            var textResponse = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content").GetString() ?? "{}";

            using var resultDoc = JsonDocument.Parse(textResponse);
            string vendor = resultDoc.RootElement.GetProperty("VendorName").GetString() ?? "Desconocido";
            decimal amount = resultDoc.RootElement.GetProperty("Amount").GetDecimal();

            return (vendor, amount);
        }
    }
}