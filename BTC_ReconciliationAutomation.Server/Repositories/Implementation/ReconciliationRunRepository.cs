using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System;

namespace BTC_ReconciliationAutomation.Server.Repositories.Implementation
{
    public class ReconciliationRunRepository : IReconciliationRunRepository
    {
        private readonly OracleDbContext _db;
        public ReconciliationRunRepository(OracleDbContext db) { _db = db; }

        public async Task<IEnumerable<reconciliation_run>> GetAllAsync()
        {
            // include related reconciliation summaries so the API returns the joined data
            return await _db.reconciliation_runs
                .Include(r => r.reconciliation_summaries)
                .Include(r => r.RUN_STATUS)
                .Include(r => r.DELIVERY_METHOD)
                .Include(r => r.EMAIL_STATUS)
                .Include(r => r.CONFIG)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<reconciliation_run?> GetByIdAsync(object id)
        {
            if (id == null) return null;

            // ensure we convert the id to the expected key type and include summaries
            int key;
            try { key = System.Convert.ToInt32(id); } catch { return null; }

            return await _db.reconciliation_runs
                .Include(r => r.reconciliation_summaries)
                .Include(r => r.generated_files)
                    .ThenInclude(f => f.FILE_TYPE)
                .Include(r => r.system_logs)
                    .ThenInclude(l => l.LOG_LEVEL)
                .Include(r => r.CONFIG)
                .Include(r => r.RUN_STATUS)
                .Include(r => r.DELIVERY_METHOD)
                .Include(r => r.EMAIL_STATUS)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RUN_ID == key);
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

        public async Task<string> RunMainReconciliationAsync()
        {
            try
            {
                var connection = _db.Database.GetDbConnection();
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "BEGIN run_main_reconciliation_log; END;";
                    command.CommandType = System.Data.CommandType.Text;

                    await command.ExecuteNonQueryAsync();
                }

                return "Reconciliation process completed successfully";
            }
            catch (Exception ex)
            {
                return $"Error running reconciliation: {ex.Message}";
            }
        }
    }
}