namespace Sim_Card_Managment.Viewmodel
{
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

        public string RenewalTypeName { get; set; } = string.Empty;
        public DateOnly? NextRenewalDate { get; set; }

        public string CreatedByUsername { get; set; } = string.Empty;


        public string? SimSerialNumber { get; set; }
        public decimal? QuotaGB { get; set; }
        public string? Notes { get; set; }

        // Used by the dashboard view to flag lines that are due/overdue
        // without recomputing dates in the razor view.
        public bool IsDueForRenewal =>
            NextRenewalDate.HasValue &&
            NextRenewalDate.Value <= DateOnly.FromDateTime(DateTime.Now);
    }
}