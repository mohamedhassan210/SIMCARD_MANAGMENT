using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;

namespace Sim_Card_Managment.Repos.FireWallTypeRepos
{
    public class FireWallTypeRepo : IFireWallTypeRepo
    {
        private readonly AppDbContext _context;

        public FireWallTypeRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FireWallType>> GetAllAsync()
        {
            return await _context.FireWallTypes
                .Include(f => f.Branches)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<FireWallType?> GetByIdAsync(int id)
        {
            return await _context.FireWallTypes
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(FireWallType fireWallType)
        {
            await _context.FireWallTypes.AddAsync(fireWallType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FireWallType fireWallType)
        {
            _context.FireWallTypes.Update(fireWallType);
            await _context.SaveChangesAsync();
        }
    }
}