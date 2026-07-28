using Microsoft.EntityFrameworkCore;
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
                .Where(s => s.Status == "Active" &&
                            (string.IsNullOrEmpty(query) || s.PhoneNumber.Contains(query) || s.SerialNumber.Contains(query)))
                .Take(6)
                .ToListAsync();
        }

        public IEnumerable<Sim> GetAll()
        {
            return _context.Sims
                .Include(s => s.ServiceProvider)
                .Include(s => s.Subscriptions!)
                    .ThenInclude(sub => sub.Employee)
                .Include(s => s.Subscriptions!)
                    .ThenInclude(sub => sub.NonEmployee)
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
                .FirstOrDefault(s => s.Id == id);
        }

        public void Add(Sim sim)
        {
            _context.Sims.Add(sim);
            _context.SaveChanges();
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
    }
}