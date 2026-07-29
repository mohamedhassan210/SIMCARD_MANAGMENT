using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;

namespace Sim_Card_Management.Repos.ItemTypeRepos
{
    public class ItemTypeRepo : IItemTypeRepo
    {
        private readonly AppDbContext _context;

        public ItemTypeRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ItemType?> GetByNameAsync(string name)
        {
            return await _context.ItemTypes
                .FirstOrDefaultAsync(it => it.Name.ToLower() == name.ToLower());
        }

        public async Task AddAsync(ItemType itemType)
        {
            await _context.ItemTypes.AddAsync(itemType);
            await _context.SaveChangesAsync();
        }
    }
}
