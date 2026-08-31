using Sim_Card_Management.Models;
using Sim_Card_Managment.Viewmodel;
namespace Sim_Card_Managment.Repos.BranchRepos
{
    public interface IBranchRepo
    {
        Task<IEnumerable<BranchListItemViewModel>> GetAllAsync(bool? isActive = null);
        Task<BranchDetailsViewModel?> GetByIdWithDetailsAsync(int id);
        Task<BranchEditViewModel?> GetForEditAsync(int id);
        Task AddAsync(BranchCreateViewModel model);
        Task UpdateAsync(BranchEditViewModel model);
        Task SoftDeleteAsync(int id);
        Task ActivateAsync(int id);
        Task<Dictionary<string, string>> GetFireWallTypeNamesByBranchNameAsync();
    }
}