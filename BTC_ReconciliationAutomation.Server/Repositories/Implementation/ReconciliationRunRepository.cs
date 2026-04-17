using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Repositories.Implementation
{
    public class ReconciliationRunRepository : IReconciliationRunRepository
    {
        private readonly OracleDbContext _db;
        public ReconciliationRunRepository(OracleDbContext db) { _db = db; }

        public async Task<IEnumerable<reconciliation_run>> GetAllAsync()
        {
            return await _db.reconciliation_runs.AsNoTracking().ToListAsync();
        }

        public async Task<reconciliation_run?> GetByIdAsync(object id)
        {
            return await _db.reconciliation_runs.FindAsync(id);
        }

        public async Task AddAsync(reconciliation_run entity)
        {
            _db.reconciliation_runs.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(reconciliation_run entity)
        {
            _db.reconciliation_runs.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var e = await _db.reconciliation_runs.FindAsync(id);
            if (e != null) { _db.reconciliation_runs.Remove(e); await _db.SaveChangesAsync(); }
        }
    }
}