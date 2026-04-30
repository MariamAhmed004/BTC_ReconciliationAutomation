using System.Collections.Generic;
using System.Threading.Tasks;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Repositories.Interfaces
{
    public interface IConfigurationRepository
    {
        Task<IEnumerable<system_configuration>> GetAllAsync();
        Task<system_configuration?> GetByIdAsync(object id);
        Task AddAsync(system_configuration entity);
        Task UpdateAsync(system_configuration entity);
        Task DeleteAsync(object id);

        /// <summary>
        /// Deactivates all currently active configurations (sets IS_ACTIVE='N', EFFECTIVE_TO=now)
        /// then inserts the new configuration as the active one.
        /// Both operations run inside a single transaction.
        /// </summary>
        Task<system_configuration> CreateNewActiveAsync(system_configuration newConfig);
    }
}