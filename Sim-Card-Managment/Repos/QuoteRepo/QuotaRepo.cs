// Repos/QuoteRepo/QuotaRepo.cs
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.QuoteRepo
{
    public class QuotaRepo : IQuotaRepo
    {
        private readonly AppDbContext _context;
        public QuotaRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quota>> GetQuotasByProviderIdAsync(int providerId)
        {
            return await _context.Quotas
                .Include(q => q.ServiceProvider)
                .Where(q => q.ServiceProviderId == providerId)
                .ToListAsync();
        }

        public IEnumerable<Quota> GetAll()
        {
            return _context.Quotas
                .Include(q => q.ServiceProvider)
                .ToList();
        }

        public Quota? GetById(int id)
        {
            // .Find() can't Include() navigation properties — must use a query.
            return _context.Quotas
                .Include(q => q.ServiceProvider)
                .FirstOrDefault(q => q.Id == id);
        }

        public void Add(Quota quota)
        {
            _context.Quotas.Add(quota);
            _context.SaveChanges();
        }

        public void Update(Quota quota)
        {
            _context.Quotas.Update(quota);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var quota = GetById(id);
            if (quota != null)
            {
                _context.Quotas.Remove(quota);
                _context.SaveChanges();
            }
        }
    }
}