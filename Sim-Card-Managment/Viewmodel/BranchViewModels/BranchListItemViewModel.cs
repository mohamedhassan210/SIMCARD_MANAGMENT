public class BranchListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool? VpnOverInternetStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public int CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;

    public int InternetLineCount { get; set; }
    public int VpnConnectionCount { get; set; }
}