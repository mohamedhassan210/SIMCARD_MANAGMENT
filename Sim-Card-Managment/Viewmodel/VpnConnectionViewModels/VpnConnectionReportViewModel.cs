namespace Sim_Card_Managment.Viewmodel
{
    public class VpnConnectionReportViewModel
    {
        public List<BranchVpnItem> Branches { get; set; } = new();
    }

    public class BranchVpnItem
    {
        public string BranchName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool? VpnOverInternetStatus { get; set; }
        public List<VpnConnectionListItemViewModel> VpnConnections { get; set; } = new();
    }
}