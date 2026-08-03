using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos.GroupRepos
{
    public interface IGroupRepo
    {
        Task<IEnumerable<Models.Group>> GetAllAsync();
        Task<Models.Group?> GetByIdWithPermissionsAsync(int id);
        Task<Group?> GetByIdAsync(int id);
        Task AddAsync(Group group);

        Task UpdateAsync(Group group);
        Task DeleteAsync(int id);
        Task AssignPermissionsAsync(int groupId, List<int> selectedPermissionIds);
        Task<Group?> GetByIdWithDetailsAsync(int id);
        Task SoftDeleteAsync(int id);
    }
}
