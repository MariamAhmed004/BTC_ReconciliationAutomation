using System.Collections.Generic;
using System.Threading.Tasks;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Repositories.Interfaces
{
    public interface IReconciliationRunRepository
    {
        Task<IEnumerable<reconciliation_run>> GetAllAsync();
        Task<reconciliation_run?> GetByIdAsync(object id);
        Task AddAsync(reconciliation_run entity);
        Task UpdateAsync(reconciliation_run entity);
        Task DeleteAsync(object id);
        Task<string> RunMainReconciliationAsync();
    }
}