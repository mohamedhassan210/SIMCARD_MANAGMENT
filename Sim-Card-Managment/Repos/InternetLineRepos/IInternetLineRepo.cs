using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.InternetLineRepos
{
    public interface IInternetLineRepo
    {
        Task<IEnumerable<InternetLineListItemViewModel>> GetAllAsync();
        Task<InternetLineDetailsViewModel?> GetByIdWithDetailsAsync(int id);
        Task<InternetLineEditViewModel?> GetForEditAsync(int id);
        Task<IEnumerable<InternetLineListItemViewModel>> GetByBranchAsync(int branchId);
        Task AddAsync(InternetLineCreateViewModel model);
        Task UpdateAsync(InternetLineEditViewModel model);
    }
}