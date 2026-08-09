namespace Sim_Card_Managment.Viewmodel
{
    public class NetworkReportViewModel
    {
        public List<BranchNetworkItem> Branches { get; set; } = new();
    }

    public class BranchNetworkItem
    {
        public string BranchName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool? VpnOverInternetStatus { get; set; }
        public List<InternetLineListItemViewModel> InternetLines { get; set; } = new();
        public List<VpnConnectionListItemViewModel> VpnConnections { get; set; } = new();
    }
}