using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;

namespace BTC_ReconciliationAutomation.Server.Repositories.Implementation
{
    public class ReconciliationStatsRepository : IReconciliationStatsRepository
    {
        private readonly OracleDbContext _db;

        public ReconciliationStatsRepository(OracleDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Fetches IGNORE_CONDITIONS from the active configuration and parses them into
        /// a list of prefix strings (strips trailing % so StartsWith can be used).
        /// Returns an empty list when no conditions are configured.
        /// </summary>
        private async Task<List<string>> GetIgnorePrefixesAsync()
        {
            var activeConfig = await _db.system_configurations
                .Where(c => c.IS_ACTIVE == "Y")
                .OrderByDescending(c => c.CREATED_AT)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(activeConfig?.IGNORE_CONDITIONS))
                return new List<string>();

            return activeConfig.IGNORE_CONDITIONS
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().TrimEnd('%'))   // "000%" → "000", "Skip%" → "Skip"
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
        }

        /// <summary>
        /// Returns true when the BNET_REF should be excluded based on the
        /// hardcoded system prefixes plus the dynamic ignore conditions from config.
        /// </summary>
        private static bool IsIgnored(string? bnetRef, List<string> ignorePrefixes)
        {
            if (string.IsNullOrEmpty(bnetRef)) return true;
            // Hardcoded system-level exclusions (always applied)
            if (bnetRef.StartsWith("1182896926")) return true;
            if (bnetRef.StartsWith("000")) return true;
            if (bnetRef.StartsWith("Skip")) return true;
            // Dynamic exclusions from active configuration IGNORE_CONDITIONS
            return ignorePrefixes.Any(p => bnetRef.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        // Query 1: Missing in ROWB (Active Siebel records not in Active ROWB)
        public async Task<int> GetMissingInRowbCountAsync()
        {
            var ignorePrefixes = await GetIgnorePrefixesAsync();

            // Step 1: Get all Active ROWB BNET_REFs (case-sensitive 'Active')
            var activeRowbRefs = await _db.ROWB_TABLEs
                .Where(r => r.STATUS == "Active")
                .Select(r => r.BNET_REF)
                .Distinct()
                .ToListAsync();

            // Step 2: Get filtered Active Siebel records
            var filteredSiebelRefs = await _db.SIEBEL_TABLEs
                .Where(s => s.STATUS_CD == "Active"
                    && s.BNET_PACK_CODE != null
                    && s.BNET_REF != null)
                .Select(s => new { s.BNET_REF, s.RENTAL_PLAN })
                .ToListAsync();

            // Step 3: Apply MINUS logic - filter patterns and exclude ROWB refs
            var missingRefs = filteredSiebelRefs
                .Where(s => (s.RENTAL_PLAN == null || !s.RENTAL_PLAN.Contains("eFax"))
                    && !IsIgnored(s.BNET_REF, ignorePrefixes)
                    && !activeRowbRefs.Contains(s.BNET_REF))
                .Select(s => s.BNET_REF)
                .ToList();

            return missingRefs.Count;
        }

        // Query 2: Not Active in Siebel but Active in ROWB
        public async Task<int> GetNotActiveInSiebelCountAsync()
        {
            var ignorePrefixes = await GetIgnorePrefixesAsync();

            // Build the subquery set: Siebel refs that are inactive/disconnected and not eFax
            var inactiveSiebelRefs = await _db.SIEBEL_TABLEs
                .Where(s => s.STATUS_CD != null && s.STATUS_CD.ToUpper() != "ACTIVE"
                    && s.DISCONNECT_DT != null
                    && s.BNET_PACK_CODE != null
                    && (s.RENTAL_PLAN == null || !s.RENTAL_PLAN.Contains("eFax")))
                .Select(s => s.BNET_REF)
                .Distinct()
                .ToListAsync();

            // JOIN ROWB and Siebel, then apply all WHERE conditions in-memory
            var joinedRecords = await (
                from r in _db.ROWB_TABLEs
                join s in _db.SIEBEL_TABLEs on r.BNET_REF equals s.BNET_REF
                where r.STATUS != null && r.STATUS.ToUpper() == "ACTIVE"
                    && r.BNET_REF != null
                select new
                {
                    r.BNET_REF,
                    r.BILLING_END_DT,
                    s.DISCONNECT_DT
                }
            ).ToListAsync();

            var count = joinedRecords
                .Where(x =>
                    !IsIgnored(x.BNET_REF, ignorePrefixes)
                    && (x.BILLING_END_DT == null || x.BILLING_END_DT != x.DISCONNECT_DT)
                    && inactiveSiebelRefs.Contains(x.BNET_REF))
                .Select(x => x.BNET_REF)
                .Distinct()
                .Count();

            return count;
        }

        // Query 3: Mismatched Packages
        public async Task<int> GetMismatchedPackagesCountAsync()
        {
            var ignorePrefixes = await GetIgnorePrefixesAsync();

            // Get all joined records
            var joinedRecords = await (from r in _db.ROWB_TABLEs
                                       join s in _db.SIEBEL_TABLEs on r.BNET_REF equals s.BNET_REF
                                       where r.ROWB_PACKAGE != s.BNET_PACK_CODE
                                           && s.BNET_PACK_CODE != null
                                           && r.ROWB_PACKAGE != null
                                           && r.BNET_REF != null
                                       select new
                                       {
                                           BNET_REF = r.BNET_REF,
                                           RENTAL_PLAN = s.RENTAL_PLAN
                                       })
                                       .ToListAsync();

            var count = joinedRecords
                .Where(x => (x.RENTAL_PLAN == null || !x.RENTAL_PLAN.Contains("eFax"))
                    && !IsIgnored(x.BNET_REF, ignorePrefixes))
                .Count();

            return count;
        }
    }
}
