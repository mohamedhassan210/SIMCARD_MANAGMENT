using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;

namespace Sim_Card_Management.Repos.DeviceSerialOperationsRepos
{
    public class DeviceSerialOperationsRepo : IDeviceSerialOperationsRepo
    {
        private readonly AppDbContext _context;

        public DeviceSerialOperationsRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DeviceSerialOperation operation)
        {
            await _context.DeviceSerialOperations.AddAsync(operation);
            await _context.SaveChangesAsync();
        }

        public async Task<DeviceSerialOperation?> GetByIdAsync(int id)
        {
            return await _context.DeviceSerialOperations
                .Include(d => d.SIM)
                .Include(d => d.CreatedBy)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<DeviceSerialOperation>> GetAllAsync()
        {
            return await _context.DeviceSerialOperations
                .Include(d => d.SIM)
                .Include(d => d.CreatedBy)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
