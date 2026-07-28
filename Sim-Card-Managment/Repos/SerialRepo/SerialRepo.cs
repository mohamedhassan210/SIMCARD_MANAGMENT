using Sim_Card_Managment.data;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class SerialRepo : ISerialRepo
    {
        private readonly AppDbContext _context;

        public SerialRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Serial>> GetAllAsync(string? serialNumber = null, int? documentId = null)
        {
            var query = _context.Serials
                .Include(s => s.DocumentDetails)
                .ThenInclude(d=>d.Document)
                .Include(s => s.CreatedBy)
                .Include(s => s.Sim)
                .Include(s => s.Usb)
                .AsQueryable();

            if (documentId.HasValue)
            {
                query = query.Where(s => s.DocumentDetails.DocumentId == documentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(serialNumber))
            {
                query = query.Where(s => s.SerialNumber.Contains(serialNumber));
            }

            return await query.ToListAsync();
        }

        public async Task<Serial?> GetByIdAsync(int id)
        {
            return await _context.Serials
                .Include(s => s.DocumentDetails)
                .ThenInclude(d => d.Document)
                .Include(s => s.Sim)
                .Include(s => s.Usb)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> ExistsAsync(string serialNumber)
        {
            return await _context.Serials.AnyAsync(s => s.SerialNumber == serialNumber);
        }

        public async Task AddAsync(Serial serial)
        {
            await _context.Serials.AddAsync(serial);
        }

        public async Task AddRangeAsync(IEnumerable<Serial> serials)
        {
            await _context.Serials.AddRangeAsync(serials);
        }

        public async Task DeleteAsync(int id)
        {
            var serial = await _context.Serials.FindAsync(id);
            if (serial != null)
            {
                _context.Serials.Remove(serial);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) >= 0;
        }
    }
}
