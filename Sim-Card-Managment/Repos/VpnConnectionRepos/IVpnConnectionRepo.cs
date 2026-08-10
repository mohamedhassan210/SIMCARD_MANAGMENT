using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.VpnConnectionRepos
{
    public interface IVpnConnectionRepo
    {
        Task<IEnumerable<VpnConnectionListItemViewModel>> GetAllAsync();
        Task<VpnConnectionDetailsViewModel?> GetByIdWithDetailsAsync(int id);
        Task<VpnConnectionEditViewModel?> GetForEditAsync(int id);
        Task<IEnumerable<VpnConnectionListItemViewModel>> GetByBranchAsync(int branchId);
        Task AddAsync(VpnConnectionCreateViewModel model);
        Task UpdateAsync(VpnConnectionEditViewModel model);
        Task<List<VpnExcelBranch>> GetForExcelAsync();
    }
}