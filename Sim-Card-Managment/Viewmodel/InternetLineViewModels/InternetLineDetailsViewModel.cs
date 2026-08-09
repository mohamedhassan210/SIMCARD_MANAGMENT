namespace Sim_Card_Managment.Viewmodel
{
    public class InternetLineDetailsViewModel
    {
        public int Id { get; set; }

        public string BranchName { get; set; } = string.Empty;
        public string ServiceProviderName { get; set; } = string.Empty;
        public string ServiceTypeName { get; set; } = string.Empty;
        public string PaymentTypeName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Bandwidth { get; set; }
        public int? RenewalDay { get; set; }
        public decimal? QuotaGB { get; set; }
        public bool Status { get; set; }
        public string? Notes { get; set; }
        public string? SimSerial { get; set; }

        public int CreatedById { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
    }
}