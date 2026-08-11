public class VpnExcelBranch
{
    public string BranchName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool? VpnOverInternetStatus { get; set; }

    public List<VpnExcelConnection> Connections { get; set; } = new();

    public string? Notes { get; set; }
}

public class VpnExcelConnection
{
    public string ConnectionTypeName { get; set; } = string.Empty;

    public string ServiceProviderName { get; set; } = string.Empty;

    public string? NID { get; set; }

    public string? LineSpeed { get; set; }

    public bool? Status { get; set; }

    public string? Notes { get; set; }
}