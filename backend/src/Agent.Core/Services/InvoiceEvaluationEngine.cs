using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agent.Core.Entities;
using Agent.Core.Interfaces;

namespace Agent.Core.Services
{
    public class InvoiceEvaluationEngine
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly IVendorRepository _vendorRepository;

        public InvoiceEvaluationEngine(
            IInvoiceRepository invoiceRepository, 
            IBudgetRepository budgetRepository, 
            IVendorRepository vendorRepository)
        {
            _invoiceRepository = invoiceRepository;
            _budgetRepository = budgetRepository;
            _vendorRepository = vendorRepository;
        }

        public async Task<EvaluationResult> EvaluateInvoiceAsync(ParsedInvoice invoice, SystemRules rules)
        {
            var reasons = new List<string>();
            var finalStatus = InvoiceStatus.Green;

            try 
            {
                if (await _invoiceRepository.IsDuplicateAsync(invoice.VendorId, invoice.Amount, invoice.ServiceMonth))
                {
                    reasons.Add("Frenada: duplicada de la factura (mismo proveedor, monto y mes de servicio).");
                    finalStatus = InvoiceStatus.Red;
                }

                var currentBudget = await _budgetRepository.GetBalanceAsync(invoice.PaymentDate.Month);
                if (currentBudget < invoice.Amount)
                {
                    reasons.Add($"Frenada: saldo del periodo {currentBudget}, monto {invoice.Amount}.");
                    finalStatus = InvoiceStatus.Red;
                }
                
                var vendor = await _vendorRepository.GetByIdAsync(invoice.VendorId);
                if (vendor == null || !vendor.IsAllowed)
                {
                    // CORRECCIÓN: Clasificar como Rojo (Frenada) e Ilegal en lugar de Amarillo
                    reasons.Add($"Frenada: ilegal, el proveedor {invoice.VendorName} no está autorizado o es desconocido.");
                    finalStatus = InvoiceStatus.Red;
                }

                if (invoice.Amount > rules.HumanApprovalThreshold)
                {
                    reasons.Add($"Escalada: monto {invoice.Amount} supera el umbral {rules.HumanApprovalThreshold}.");
                    finalStatus = finalStatus == InvoiceStatus.Red ? InvoiceStatus.Red : InvoiceStatus.Yellow;
                }

                if (invoice.DueDate < DateTime.UtcNow.Date)
                {
                    reasons.Add($"Escalada: vencida el {invoice.DueDate:yyyy-MM-dd}.");
                    finalStatus = finalStatus == InvoiceStatus.Red ? InvoiceStatus.Red : InvoiceStatus.Yellow;
                }

                if (invoice.HasAmbiguousData)
                {
                    reasons.Add($"Escalada: no se pudo leer con certeza {invoice.AmbiguousField}.");
                    finalStatus = finalStatus == InvoiceStatus.Red ? InvoiceStatus.Red : InvoiceStatus.Yellow;
                }

                if (finalStatus == InvoiceStatus.Green)
                {
                    reasons.Add("Pagar: beneficiario permitido, monto <= límite, coincide con lo acordado, saldo suficiente.");
                }
            }
            catch(Exception ex)
            {
                reasons.Add($"Error interno de evaluación: {ex.Message}");
                finalStatus = InvoiceStatus.Yellow; 
            }

            return new EvaluationResult 
            { 
                Status = finalStatus, 
                Explanations = reasons 
            };
        }
    }

    public enum InvoiceStatus { Green, Yellow, Red }

    public class EvaluationResult
    {
        public InvoiceStatus Status { get; set; }
        public List<string> Explanations { get; set; } = new();
    }
}