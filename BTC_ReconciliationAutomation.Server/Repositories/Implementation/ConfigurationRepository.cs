using System;
using System.Collections.Generic;
using System.Linq;
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
            return await _db.system_configurations
                .OrderByDescending(c => c.IS_ACTIVE == "Y" ? 1 : 0)
                .ThenByDescending(c => c.EFFECTIVE_FROM)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<system_configuration?> GetByIdAsync(object id)
        {
            return await _db.system_configurations.FindAsync(id);
        }

        public async Task<bool> IsPurgeJobEnabledAsync()
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ENABLED FROM USER_SCHEDULER_JOBS WHERE JOB_NAME = 'PURGE_LOGS_JOB'";

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString()?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? false;
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

        public async Task<system_configuration> CreateNewActiveAsync(system_configuration newConfig)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Deactivate all currently active configurations
                var activeConfigs = await _db.system_configurations
                    .Where(c => c.IS_ACTIVE == "Y")
                    .ToListAsync();

                var now = DateTime.Now;
                foreach (var config in activeConfigs)
                {
                    config.IS_ACTIVE = "N";
                    config.EFFECTIVE_TO = now;
                }

                // Prepare the new configuration
                newConfig.IS_ACTIVE = "Y";
                newConfig.EFFECTIVE_FROM = now;
                newConfig.EFFECTIVE_TO = null;
                newConfig.CREATED_AT = now;

                _db.system_configurations.Add(newConfig);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Enable or disable the Oracle purge scheduler job
                // based on whether log retention is configured in the new config 
                var toggleFlag = newConfig.DAYS_TO_DELETE_AUDITLOGS.HasValue ? "Y" : "N";
                await _db.Database.ExecuteSqlRawAsync(
                    "BEGIN TOGGLE_PURGE_JOB(:p_enable); END;",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_enable", toggleFlag)
                );

                return newConfig;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}