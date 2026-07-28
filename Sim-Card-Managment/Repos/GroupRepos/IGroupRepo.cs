namespace Sim_Card_Managment.Repos.GroupRepos
{
    public interface IGroupRepo
    {
        Task<IEnumerable<Models.Group>> GetAllAsync();
        Task<Models.Group?> GetByIdWithPermissionsAsync(int id);
        Task AssignPermissionsAsync(int groupId, IEnumerable<int> permissionIds);
    }
}
