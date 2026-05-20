using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Repositories.Implementation
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly OracleDbContext _db;
        public SystemLogRepository(OracleDbContext db) { _db = db; }

        public async Task<IEnumerable<system_log>> GetAllAsync()
        {
            return await _db.system_logs
                .Include(s => s.LOG_LEVEL)
                .Include(s => s.RUN)
                .OrderByDescending(s => s.CREATED_AT)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<system_log?> GetByIdAsync(object id)
        {
            return await _db.system_logs.FindAsync(id);
        }

        public async Task AddAsync(system_log entity)
        {
            _db.system_logs.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(system_log entity)
        {
            _db.system_logs.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var e = await _db.system_logs.FindAsync(id);
            if (e != null) { _db.system_logs.Remove(e); await _db.SaveChangesAsync(); }
        }

        public async Task<IEnumerable<log_level>> GetAllLevelsAsync()
        {
            return await _db.log_levels.AsNoTracking().ToListAsync();
        }
    }
}