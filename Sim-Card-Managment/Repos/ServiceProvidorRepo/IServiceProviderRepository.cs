namespace Sim_Card_Managment.Repos
{
    public interface IServiceProviderRepository
    {
        Task<IEnumerable<Models.ServiceProvider>> GetAllAsync();
        Task<Models.ServiceProvider?> GetByIdAsync(Guid id);
        Task AddAsync(Models.ServiceProvider provider);
        Task UpdateAsync(Models.ServiceProvider provider);
        Task DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
