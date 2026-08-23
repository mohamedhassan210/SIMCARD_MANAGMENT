using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.MailConfigurationRepos
{
    public interface IMailConfigurationRepo
    {
        Task<List<MailConfiguration>> GetAllAsync();
        Task<MailConfiguration?> GetByIdAsync(int id);
        Task<MailConfiguration?> GetActiveAsync();
        Task AddAsync(MailConfiguration config);
        Task UpdateAsync(MailConfiguration config);
        Task DeleteAsync(int id);
        Task SetActiveAsync(int id);
    }
}