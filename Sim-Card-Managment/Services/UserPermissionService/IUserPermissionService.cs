namespace Sim_Card_Managment.Services
{
    public interface IUserPermissionService
    {
        Task<HashSet<string>> GetPermissionKeysAsync();
        Task<bool> HasPermissionAsync(string controller, string action);
    }
}