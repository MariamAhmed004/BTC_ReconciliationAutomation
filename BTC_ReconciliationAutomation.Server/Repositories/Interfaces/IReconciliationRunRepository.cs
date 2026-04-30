using System.Collections.Generic;
using System.Threading.Tasks;
using BTC_ReconciliationAutomation.Server.Models;

namespace BTC_ReconciliationAutomation.Server.Repositories.Interfaces
{
    public class ReconciliationFileResult
    {
        public string? File1Name { get; set; }
        public string? File1Content { get; set; }
        public string? File2Name { get; set; }
        public string? File2Content { get; set; }
        public string? File3Name { get; set; }
        public string? File3Content { get; set; }
        public string? File4Name { get; set; }
        public string? File4Content { get; set; }
    }

    public interface IReconciliationRunRepository
    {
        Task<IEnumerable<reconciliation_run>> GetAllAsync();
        Task<reconciliation_run?> GetByIdAsync(object id);
        Task AddAsync(reconciliation_run entity);
        Task UpdateAsync(reconciliation_run entity);
        Task DeleteAsync(object id);
        Task<ReconciliationFileResult> RunMainReconciliationAsync(bool isTriggered, string triggeredBy);
    }
}