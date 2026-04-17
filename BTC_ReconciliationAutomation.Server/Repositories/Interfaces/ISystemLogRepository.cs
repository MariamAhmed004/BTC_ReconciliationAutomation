using System.Collections.Generic;
using System.Threading.Tasks;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Repositories.Interfaces
{
    public interface ISystemLogRepository
    {
        Task<IEnumerable<system_log>> GetAllAsync();
        Task<system_log?> GetByIdAsync(object id);
        Task AddAsync(system_log entity);
        Task UpdateAsync(system_log entity);
        Task DeleteAsync(object id);
    }
}