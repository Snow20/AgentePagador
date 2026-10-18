using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Entities;
using Agent.Core.Interfaces;
using Agent.Infrastructure.Data;

namespace Agent.Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AgentDbContext _context;

        public InvoiceRepository(AgentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsDuplicateAsync(Guid vendorId, decimal amount, string serviceMonth)
        {
            return await Task.FromResult(false);
        }

        public async Task SaveRawAsync(RawInvoice invoice)
        {
            _context.RawInvoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        // Implementación actualizada con los nuevos parámetros
        public async Task UpdateInvoiceResultAsync(Guid id, string status, string color, string explanation, decimal amount, bool isDuplicate, bool isIllegal, bool needsReview)
        {
            var invoice = await _context.RawInvoices.FindAsync(id);
            if (invoice != null)
            {
                invoice.Status = status;
                invoice.Color = color;
                invoice.Explanation = explanation;
                
                // Asignación de las nuevas variables de negocio
                invoice.Amount = amount;
                invoice.IsDuplicate = isDuplicate;
                invoice.IsIllegal = isIllegal;
                invoice.NeedsReview = needsReview;

                _context.RawInvoices.Update(invoice);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<RawInvoice>> GetAllAsync()
        {
            return await _context.RawInvoices.ToListAsync();
        }
    }
}