using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Repositories.Implementation
{
    public class SummaryRepository : ISummaryRepository
    {
        private readonly OracleDbContext _db;
        public SummaryRepository(OracleDbContext db) { _db = db; }

        public async Task<IEnumerable<reconciliation_summary>> GetAllAsync()
        {
            return await _db.reconciliation_summaries.AsNoTracking().ToListAsync();
        }

        public async Task<reconciliation_summary?> GetByIdAsync(object id)
        {
            return await _db.reconciliation_summaries.FindAsync(id);
        }

        public async Task AddAsync(reconciliation_summary entity)
        {
            _db.reconciliation_summaries.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(reconciliation_summary entity)
        {
            _db.reconciliation_summaries.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var e = await _db.reconciliation_summaries.FindAsync(id);
            if (e != null) { _db.reconciliation_summaries.Remove(e); await _db.SaveChangesAsync(); }
        }
    }
}