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
            return await _db.reconciliation_runs
                .Include(r => r.reconciliation_summaries)
                .Include(r => r.RUN_STATUS)
                .Include(r => r.DELIVERY_METHOD)
                .Include(r => r.EMAIL_STATUS)
                .Include(r => r.CONFIG)
                .OrderByDescending(r => r.RUN_DATE)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<reconciliation_run?> GetByIdAsync(object id)
        {
            if (id == null) return null;

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

        public async Task<ReconciliationFileResult> RunMainReconciliationAsync(bool isTriggered, string triggeredBy)
        {
            var connection = _db.Database.GetDbConnection() as OracleConnection
                ?? throw new InvalidOperationException("Could not obtain OracleConnection.");

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand() as OracleCommand
                ?? throw new InvalidOperationException("Could not create OracleCommand.");

            // ODP.NET managed driver cannot bind Oracle BOOLEAN directly.
            // Use an anonymous PL/SQL block to convert the INT bind variable to BOOLEAN locally.
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = @"
BEGIN
    run_main_reconciliation_log(
        p_is_triggered   => CASE WHEN :p_is_triggered = 1 THEN TRUE ELSE FALSE END,
        p_triggered_by   => :p_triggered_by,
        p_out_file1_name => :p_out_file1_name,
        p_out_file1_clob => :p_out_file1_clob,
        p_out_file2_name => :p_out_file2_name,
        p_out_file2_clob => :p_out_file2_clob,
        p_out_file3_name => :p_out_file3_name,
        p_out_file3_clob => :p_out_file3_clob,
        p_out_file4_name => :p_out_file4_name,
        p_out_file4_clob => :p_out_file4_clob
    );
END;";

            // IN parameters
            command.Parameters.Add(new OracleParameter("p_is_triggered", OracleDbType.Int32)
            {
                Direction = System.Data.ParameterDirection.Input,
                Value = isTriggered ? 1 : 0
            });
            command.Parameters.Add(new OracleParameter("p_triggered_by", OracleDbType.Varchar2, 50)
            {
                Direction = System.Data.ParameterDirection.Input,
                Value = triggeredBy ?? "Automatically"
            });

            // OUT parameters — file names
            var pFile1Name = new OracleParameter("p_out_file1_name", OracleDbType.Varchar2, 200) { Direction = System.Data.ParameterDirection.Output };
            var pFile2Name = new OracleParameter("p_out_file2_name", OracleDbType.Varchar2, 200) { Direction = System.Data.ParameterDirection.Output };
            var pFile3Name = new OracleParameter("p_out_file3_name", OracleDbType.Varchar2, 200) { Direction = System.Data.ParameterDirection.Output };
            var pFile4Name = new OracleParameter("p_out_file4_name", OracleDbType.Varchar2, 200) { Direction = System.Data.ParameterDirection.Output };

            // OUT parameters — file CLOBs
            var pFile1Clob = new OracleParameter("p_out_file1_clob", OracleDbType.Clob) { Direction = System.Data.ParameterDirection.Output };
            var pFile2Clob = new OracleParameter("p_out_file2_clob", OracleDbType.Clob) { Direction = System.Data.ParameterDirection.Output };
            var pFile3Clob = new OracleParameter("p_out_file3_clob", OracleDbType.Clob) { Direction = System.Data.ParameterDirection.Output };
            var pFile4Clob = new OracleParameter("p_out_file4_clob", OracleDbType.Clob) { Direction = System.Data.ParameterDirection.Output };

            command.Parameters.AddRange(new[]
            {
                pFile1Name, pFile1Clob,
                pFile2Name, pFile2Clob,
                pFile3Name, pFile3Clob,
                pFile4Name, pFile4Clob
            });

            await command.ExecuteNonQueryAsync();

            static string? ReadClob(OracleParameter p)
            {
                if (p.Value is Oracle.ManagedDataAccess.Types.OracleClob clob && !clob.IsNull)
                    return clob.Value;
                return null;
            }

            return new ReconciliationFileResult
            {
                File1Name = pFile1Name.Value?.ToString(),
                File1Content = ReadClob(pFile1Clob),
                File2Name = pFile2Name.Value?.ToString(),
                File2Content = ReadClob(pFile2Clob),
                File3Name = pFile3Name.Value?.ToString(),
                File3Content = ReadClob(pFile3Clob),
                File4Name = pFile4Name.Value?.ToString(),
                File4Content = ReadClob(pFile4Clob),
            };
        }
    }
}