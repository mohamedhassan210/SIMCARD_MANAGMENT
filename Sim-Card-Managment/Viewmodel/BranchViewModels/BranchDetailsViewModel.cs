using Sim_Card_Managment.Viewmodel;

public class BranchDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool? VpnOverInternetStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public int CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;

    public List<InternetLineListItemViewModel> InternetLines { get; set; } = new();
    public List<VpnConnectionListItemViewModel> VpnConnections { get; set; } = new();
}