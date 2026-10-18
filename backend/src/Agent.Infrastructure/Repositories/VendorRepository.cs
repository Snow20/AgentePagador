using System;
using System.Threading.Tasks;
using Agent.Core.Entities;
using Agent.Core.Interfaces;

namespace Agent.Infrastructure.Repositories
{
    public class VendorRepository : IVendorRepository
    {
        public Task<Vendor> GetByIdAsync(Guid id)
        {
            // Retorna null para cualquier Guid no registrado explícitamente, 
            // forzando al InvoiceEvaluationEngine a clasificarlo como proveedor ilegal/desconocido.
            return Task.FromResult<Vendor>(null);
        }
    }
}