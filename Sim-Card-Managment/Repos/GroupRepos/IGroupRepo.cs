namespace Sim_Card_Managment.Repos.GroupRepos
{
    public interface IGroupRepo
    {
        Task<IEnumerable<Models.Group>> GetAllAsync();
        Task<Models.Group?> GetByIdWithPermissionsAsync(Guid id);
        Task AssignPermissionsAsync(Guid groupId, IEnumerable<Guid> permissionIds);
    }
}
