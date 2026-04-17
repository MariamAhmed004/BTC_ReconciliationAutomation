using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Repositories.Implementation
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly OracleDbContext _db;
        public ConfigurationRepository(OracleDbContext db) { _db = db; }

        public async Task<IEnumerable<system_configuration>> GetAllAsync()
        {
            return await _db.system_configurations.AsNoTracking().ToListAsync();
        }

        public async Task<system_configuration?> GetByIdAsync(object id)
        {
            return await _db.system_configurations.FindAsync(id);
        }

        public async Task AddAsync(system_configuration entity)
        {
            _db.system_configurations.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(system_configuration entity)
        {
            _db.system_configurations.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var e = await _db.system_configurations.FindAsync(id);
            if (e != null) { _db.system_configurations.Remove(e); await _db.SaveChangesAsync(); }
        }
    }
}