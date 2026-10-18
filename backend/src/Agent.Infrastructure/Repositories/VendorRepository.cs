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
            // Solo aprueba el ID determinista asignado a proveedores legítimos
            if (id == new Guid("11111111-1111-1111-1111-111111111111"))
            {
                return Task.FromResult(new Vendor 
                { 
                    Id = id, 
                    IsAllowed = true, 
                    RegisteredIban = "ES9121000418401234567891" 
                });
            }

            // Cualquier otro ID (como Guid.Empty de Meridian) es rechazado
            return Task.FromResult<Vendor>(null);
        }
    }
}