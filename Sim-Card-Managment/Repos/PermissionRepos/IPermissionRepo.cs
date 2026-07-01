using Sim_Card_Managment.Models;

public interface IPermissionRepo
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(Guid id);
}