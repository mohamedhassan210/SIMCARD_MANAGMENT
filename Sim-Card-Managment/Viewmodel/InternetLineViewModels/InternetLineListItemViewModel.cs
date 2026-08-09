public class InternetLineListItemViewModel
{
    public int Id { get; set; }

    public string BranchName { get; set; } = string.Empty;
    public string ServiceProviderName { get; set; } = string.Empty;
    public string ServiceTypeName { get; set; } = string.Empty;
    public string PaymentTypeName { get; set; } = string.Empty;

    public string? Bandwidth { get; set; }
    public string? PhoneNumber { get; set; }
    public bool Status { get; set; }

    public int CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
}