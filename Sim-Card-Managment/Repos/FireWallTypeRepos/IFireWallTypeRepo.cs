using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Repos.FireWallTypeRepos
{
    public interface IFireWallTypeRepo
    {
        Task<IEnumerable<FireWallType>> GetAllAsync();
        Task<FireWallType?> GetByIdAsync(int id);
        Task AddAsync(FireWallType fireWallType);
        Task UpdateAsync(FireWallType fireWallType);
    }
}