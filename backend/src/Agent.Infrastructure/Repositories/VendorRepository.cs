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
            // Simula un proveedor autorizado en el sistema
            return Task.FromResult(new Vendor 
            { 
                Id = id, 
                IsAllowed = true, 
                RegisteredIban = "ES9121000418401234567891" 
            });
        }
    }
}