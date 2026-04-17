using System.Collections.Generic;
using System.Threading.Tasks;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Repositories.Interfaces
{
    public interface ISummaryRepository
    {
        Task<IEnumerable<reconciliation_summary>> GetAllAsync();
        Task<reconciliation_summary?> GetByIdAsync(object id);
        Task AddAsync(reconciliation_summary entity);
        Task UpdateAsync(reconciliation_summary entity);
        Task DeleteAsync(object id);
    }
}