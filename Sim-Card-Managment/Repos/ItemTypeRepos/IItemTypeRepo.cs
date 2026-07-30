using Sim_Card_Management.Models;

namespace Sim_Card_Management.Repos.ItemTypeRepos
{
    public interface IItemTypeRepo
    {
        Task<ItemType?> GetByNameAsync(string name);
        Task AddAsync(ItemType itemType);
    }
}
