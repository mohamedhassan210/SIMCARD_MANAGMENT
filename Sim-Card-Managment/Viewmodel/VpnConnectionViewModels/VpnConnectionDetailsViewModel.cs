namespace Sim_Card_Managment.Viewmodel
{
    public class VpnConnectionDetailsViewModel
    {
        public int Id { get; set; }

        public string BranchName { get; set; } = string.Empty;
        public string ConnectionTypeName { get; set; } = string.Empty;
        public string ServiceProviderName { get; set; } = string.Empty;

        public string? NID { get; set; }
        public string? LineSpeed { get; set; }
        public bool? Status { get; set; }
        public string? Notes { get; set; }
       
        public string CreatedByUsername { get; set; } = string.Empty;
    }
}