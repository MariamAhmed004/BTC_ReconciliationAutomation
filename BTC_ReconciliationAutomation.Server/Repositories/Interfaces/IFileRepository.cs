using System.Collections.Generic;
using System.Threading.Tasks;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Repositories.Interfaces
{
    public interface IFileRepository
    {
        Task<IEnumerable<generated_file>> GetAllAsync();
        Task<IEnumerable<generated_file>> GetLatestFilesAsync(int count = 50);
        Task<generated_file?> GetByIdAsync(object id);
        Task AddAsync(generated_file entity);
        Task UpdateAsync(generated_file entity);
        Task DeleteAsync(object id);
    }
}