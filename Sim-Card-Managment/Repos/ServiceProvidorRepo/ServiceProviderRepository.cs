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

        public async Task<Models.ServiceProvider?> GetByIdAsync(int id)
        {
            return await _context.ServiceProviders.FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Loads a provider with its Sims/Usbs, including each device's active
        /// subscription holder (Employee or NonEmployee), for the Details page.
        /// </summary>
        public async Task<Models.ServiceProvider?> GetByIdWithDevicesAsync(int id)
        {
            return await _context.ServiceProviders
                .Include(p => p.Quotas)
                .Include(p => p.Sims)
                    .ThenInclude(s => s.Subscriptions)
                        .ThenInclude(sub => sub.Employee)
                .Include(p => p.Sims)
                    .ThenInclude(s => s.Subscriptions)
                        .ThenInclude(sub => sub.NonEmployee)
                .Include(p => p.Sims)
                    .ThenInclude(s => s.DeviceStatuses)
                        .ThenInclude(ds => ds.StatusType)
                .Include(p => p.Usbs)
                    .ThenInclude(u => u.Subscriptions)
                        .ThenInclude(sub => sub.Employee)
                .Include(p => p.Usbs)
                    .ThenInclude(u => u.Subscriptions)
                        .ThenInclude(sub => sub.NonEmployee)
                .Include(p => p.Usbs)
                    .ThenInclude(u => u.DeviceStatuses)
                        .ThenInclude(ds => ds.StatusType)
                .FirstOrDefaultAsync(p => p.Id == id);
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

        // Soft delete: flips IsActive to false instead of removing the row.
        public async Task DeleteAsync(int id)
        {
            var provider = await _context.ServiceProviders.FindAsync(id);
            if (provider != null)
            {
                provider.IsActive = false;
                _context.ServiceProviders.Update(provider);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) >= 0;
        }
        public async Task ActivateAsync(int id)
        {
            var provider = await _context.ServiceProviders
                .AsNoTracking()  // ← avoid tracking conflicts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (provider != null)
            {
                provider.IsActive = true;
                _context.ServiceProviders.Update(provider);
                await _context.SaveChangesAsync();
            }
        }
    }
}