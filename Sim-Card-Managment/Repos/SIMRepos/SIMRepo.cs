using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.CodeCoverage;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos
{
    public class SIMRepo : ISIMRepo
    {
        private readonly AppDbContext _context;
        public SIMRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Sim>> GetAvailableSimsAsync()
        {
            return await _context.Sims
                .Where(s => !_context.Serials.Any(ser => ser.SimId == s.Id))
                .ToListAsync();
        }
        public async Task<IEnumerable<Sim>> GetAvailableSimsAsync(string query)
        {
            return await _context.Sims
                .Include(s => s.ServiceProvider)
                .Where(s => s.IsActive &&
                            (string.IsNullOrEmpty(query) || s.PhoneNumber.Contains(query) || s.SerialNumber.Contains(query)))
                .Take(6)
                .ToListAsync();
        }
        public async Task UpdateAsync(Sim sim)
        {
            _context.Sims.Update(sim);
            await _context.SaveChangesAsync();
        }
        public async Task<Sim?> GetByIdAsync(int id)
        {
            return await _context.Sims
                .Include(s => s.ServiceProvider)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public IEnumerable<Sim> GetAll()
        {
            return _context.Sims
                .Include(s => s.ServiceProvider)
                .Include(s => s.Subscriptions!)
                    .ThenInclude(sub => sub.Employee)
                .Include(s => s.Subscriptions!)
                    .ThenInclude(sub => sub.NonEmployee)
                .Include(s => s.DeviceStatuses)
                    .ThenInclude(ds => ds.StatusType)
                .ToList();
        }
        public Sim? GetById(int id)
        {
            return _context.Sims
                .Include(s => s.ServiceProvider)
                .Include(s => s.Subscriptions!)
                    .ThenInclude(sub => sub.Employee)
                .Include(s => s.Subscriptions!)
                    .ThenInclude(sub => sub.NonEmployee)
                .Include(s => s.Subscriptions!)
                    .ThenInclude(sub => sub.Quota)
                .Include(s => s.DeviceStatuses)
                    .ThenInclude(ds => ds.StatusType)
                .FirstOrDefault(s => s.Id == id);
        }
        public void Add(Sim sim)
        {
            _context.Sims.Add(sim);
            _context.SaveChanges();
        }
        public async Task AddAsync(Sim sim)
        {
            await _context.Sims.AddAsync(sim);
            await _context.SaveChangesAsync();
        }
        public async Task<Sim?> GetBySerialNumberAsync(string serialNumber)
        {
            return await _context.Sims
                .Include(s => s.ServiceProvider)
                .FirstOrDefaultAsync(s => s.SerialNumber == serialNumber);
        }
        public async Task<IEnumerable<Sim>> SearchAsync(string searchTerm)
        {
            var searchLower = searchTerm.ToLower().Trim();
            return await _context.Sims
                .Include(s => s.ServiceProvider)
                .Where(s => (s.PhoneNumber != null && s.PhoneNumber.Contains(searchTerm))
                         || (s.SerialNumber != null && s.SerialNumber.ToLower().Contains(searchLower)))
                .AsNoTracking()
                .ToListAsync();
        }
        public void Update(Sim sim)
        {
            _context.Sims.Update(sim);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var sim = GetById(id);
            if (sim != null)
            {
                _context.Sims.Remove(sim);
                _context.SaveChanges();
            }
        }
        public async Task<IEnumerable<Sim>> GetAssignableSimsAsync(string query)
        {
            return await _context.Sims
                .Include(s => s.ServiceProvider)
                .Where(s => s.IsActive &&
                            !s.Subscriptions.Any(sub => sub.EndDate == null || sub.EndDate > DateTime.Now) &&
                            (string.IsNullOrEmpty(query) || s.PhoneNumber.Contains(query) || s.SerialNumber.Contains(query)))
                .Take(6)
                .ToListAsync();
        }
        public async Task<List<Sim>> GetAssignableSimsForInternetLineAsync(string? query, int? excludeLineId = null)
        {
            var simsQuery = _context.Sims
                .Include(s => s.ServiceProvider)
                .Where(s => s.IsActive &&
                            !_context.InternetLines.Any(il =>
                                il.SimId == s.Id &&
                                (excludeLineId == null || il.Id != excludeLineId)));

            if (!string.IsNullOrWhiteSpace(query))
            {
                simsQuery = simsQuery.Where(s =>
                    s.PhoneNumber.Contains(query) ||
                    s.SerialNumber.Contains(query));
            }

            return await simsQuery
                .OrderBy(s => s.PhoneNumber)
                .Take(20)
                .ToListAsync();
        }
    }
}