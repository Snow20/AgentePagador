using System.Threading.Tasks;
using Agent.Core.Interfaces;

namespace Agent.Infrastructure.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        public Task<decimal> GetBalanceAsync(int month)
        {
            // Simula un presupuesto disponible de 10,000 USD para pruebas
            return Task.FromResult(10000m);
        }
    }
}