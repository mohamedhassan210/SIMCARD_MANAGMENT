namespace Sim_Card_Managment.Viewmodel
{
    public class InternetLineDashboardItemViewModel
    {
        public int Id { get; set; }

        public string BranchName { get; set; } = string.Empty;
        public string ServiceProviderName { get; set; } = string.Empty;
        public string PaymentTypeName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public DateOnly? LastRenewalDate { get; set; }
        public DateOnly? NextRenewalDate { get; set; }

        // Null means no RenewalType is assigned to this line - the view
        // uses this to hide the Renew button and show a placeholder instead.
        public string? RenewalTypeName { get; set; }

        public bool IsDueForRenewal =>
            NextRenewalDate.HasValue &&
            NextRenewalDate.Value <= DateOnly.FromDateTime(DateTime.Now);
    }
}