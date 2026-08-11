public class InternetLineExcelViewModel
{
    public string BranchName { get; set; } = string.Empty;

    public List<InternetLineExcelItem> InternetLines { get; set; } = new();
}

public class InternetLineExcelItem
{
    public string ServiceProviderName { get; set; } = string.Empty;
    public string PaymentTypeName { get; set; } = string.Empty;
    public string ServiceTypeName { get; set; } = string.Empty;

    public string? SimSerialNumber { get; set; }
    public string? PhoneNumber { get; set; }

    public int? RenewalDay { get; set; }
    public decimal? QuotaGB { get; set; }

    public string? Bandwidth { get; set; }
    public bool Status { get; set; }

    public string? Notes { get; set; }
}