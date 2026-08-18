namespace Sim_Card_Managment.Viewmodel
{
    using Sim_Card_Managment.Viewmodel;
    public class BranchDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool? VpnOverInternetStatus { get; set; }
        public string? SiteCode { get; set; }
        public string? BranchCode { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public List<string> FireWallTypeNames { get; set; } = new();
        public List<InternetLineListItemViewModel> InternetLines { get; set; } = new();
        public List<VpnConnectionListItemViewModel> VpnConnections { get; set; } = new();
    }
}