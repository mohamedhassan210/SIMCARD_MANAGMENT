using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;

namespace Sim_Card_Managment.Repositories
{
    public class ServiceProviderRepository : IServiceProviderRepository
    {
        private readonly AppDbContext _context;

        public ServiceProviderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Models.ServiceProvider>> GetAllAsync()
        {
            return await _context.ServiceProviders.OrderBy(p => p.DisplayName).ToListAsync();
        }

        public async Task<Models.ServiceProvider?> GetByIdAsync(Guid id)
        {
            return await _context.ServiceProviders.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Models.ServiceProvider provider)
        {
            await _context.ServiceProviders.AddAsync(provider);
        }

        public async Task UpdateAsync(Models.ServiceProvider provider)
        {
            _context.ServiceProviders.Update(provider);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var provider = await _context.ServiceProviders.FindAsync(id);
            if (provider != null)
            {
                _context.ServiceProviders.Remove(provider);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) >= 0;
        }
    }
}