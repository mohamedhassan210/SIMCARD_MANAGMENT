namespace Sim_Card_Managment.Repos
{
    public interface IServiceProviderRepository
    {
        Task<IEnumerable<Models.ServiceProvider>> GetAllAsync();
        Task<Models.ServiceProvider?> GetByIdAsync(int id);
        Task<Models.ServiceProvider?> GetByIdWithDevicesAsync(int id);
        Task AddAsync(Models.ServiceProvider provider);
        Task UpdateAsync(Models.ServiceProvider provider);
        Task DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
        Task ActivateAsync(int id);
    }
}