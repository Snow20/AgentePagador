using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agent.Core.Entities;

namespace Agent.Core.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<bool> IsDuplicateAsync(Guid vendorId, decimal amount, string serviceMonth);
        Task SaveRawAsync(RawInvoice invoice);
        
        // Firma actualizada con los nuevos parámetros
        Task UpdateInvoiceResultAsync(Guid id, string status, string color, string explanation, decimal amount, bool isDuplicate, bool isIllegal, bool needsReview);
        
        Task<IEnumerable<RawInvoice>> GetAllAsync();
    }

    public interface IBudgetRepository
    {
        Task<decimal> GetBalanceAsync(int month);
    }

    public interface IVendorRepository
    {
        Task<Vendor> GetByIdAsync(Guid id);
    }

    public interface IMessagePublisher
    {
        Task PublishAsync(string routingKey, object message);
    }
}