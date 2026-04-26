using System.Threading.Tasks;

namespace BTC_ReconciliationAutomation.Server.Repositories.Interfaces
{
    public interface IReconciliationStatsRepository
    {
        Task<int> GetMissingInRowbCountAsync();
        Task<int> GetNotActiveInSiebelCountAsync();
        Task<int> GetMismatchedPackagesCountAsync();
    }
}
