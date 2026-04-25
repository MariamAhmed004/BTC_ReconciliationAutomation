using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BTC_ReconciliationAutomation.Server.Models;
using BTC_ReconciliationAutomation.Server.Repositories.Interfaces;
using System.Linq;

namespace BTC_ReconciliationAutomation.Server.Repositories.Implementation
{
    public class FileRepository : IFileRepository
    {
        private readonly OracleDbContext _db;
        public FileRepository(OracleDbContext db) { _db = db; }

        public async Task<IEnumerable<generated_file>> GetAllAsync()
        {
            return await _db.generated_files.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<generated_file>> GetLatestFilesAsync(int count = 50)
        {
            return await _db.generated_files
                .Include(f => f.EMAIL_STATUS)
                .Include(f => f.DELIVERY_METHOD)
                .Include(f => f.RUN)
                .OrderByDescending(f => f.FILE_ID)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<generated_file?> GetByIdAsync(object id)
        {
            return await _db.generated_files.FindAsync(id);
        }

        public async Task AddAsync(generated_file entity)
        {
            _db.generated_files.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(generated_file entity)
        {
            _db.generated_files.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var e = await _db.generated_files.FindAsync(id);
            if (e != null) { _db.generated_files.Remove(e); await _db.SaveChangesAsync(); }
        }
    }
}