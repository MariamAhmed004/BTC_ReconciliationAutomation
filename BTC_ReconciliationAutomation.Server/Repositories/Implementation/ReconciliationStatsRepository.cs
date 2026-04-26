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

        // Query 1: Missing in ROWB (Active Siebel records not in Active ROWB)
        public async Task<int> GetMissingInRowbCountAsync()
        {
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
                .Where(s => s.RENTAL_PLAN != null && !s.RENTAL_PLAN.Contains("eFax")
                    && !s.BNET_REF.StartsWith("1182896926")
                    && !s.BNET_REF.StartsWith("000")
                    && !s.BNET_REF.StartsWith("Skip")
                    && !activeRowbRefs.Contains(s.BNET_REF))
                .Select(s => s.BNET_REF)
                .ToList();

            return missingRefs.Count;
        }

        // Query 2: Not Active in Siebel but Active in ROWB
        public async Task<int> GetNotActiveInSiebelCountAsync()
        {
            // Get inactive/disconnected Siebel BNET_REFs (excluding eFax)
            var allInactiveSiebel = await _db.SIEBEL_TABLEs
                .Where(s => s.STATUS_CD != null && s.STATUS_CD.ToUpper() != "ACTIVE"
                    && s.DISCONNECT_DT != null
                    && s.BNET_PACK_CODE != null)
                .ToListAsync();

            var inactiveSiebelRefs = allInactiveSiebel
                .Where(s => s.RENTAL_PLAN != null && !s.RENTAL_PLAN.Contains("eFax"))
                .Select(s => s.BNET_REF)
                .ToList();

            // Get active ROWB records
            var activeRowbRecords = await _db.ROWB_TABLEs
                .Where(r => r.STATUS != null && r.STATUS.ToUpper() == "ACTIVE"
                    && r.BNET_REF != null)
                .ToListAsync();

            // Filter in-memory and count distinct
            var count = activeRowbRecords
                .Where(r => !r.BNET_REF.StartsWith("1182896926")
                    && !r.BNET_REF.StartsWith("000")
                    && !r.BNET_REF.StartsWith("Skip")
                    && !inactiveSiebelRefs.Contains(r.BNET_REF))
                .Select(r => r.BNET_REF)
                .Distinct()
                .Count();

            return count;
        }

        // Query 3: Mismatched Packages
        public async Task<int> GetMismatchedPackagesCountAsync()
        {
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

            // Filter in-memory to avoid Oracle boolean issues
            var count = joinedRecords
                .Where(x => x.RENTAL_PLAN != null && !x.RENTAL_PLAN.Contains("eFax")
                    && !x.BNET_REF.StartsWith("1182896926")
                    && !x.BNET_REF.StartsWith("000")
                    && !x.BNET_REF.StartsWith("Skip"))
                .Count();

            return count;
        }
    }
}
