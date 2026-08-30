namespace Sim_Card_Managment.Viewmodel
{
    public class ServiceProviderDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
        public string? LogoPath { get; set; }   // NEW
        public List<QuotaDisplayViewModel> Quotas { get; set; } = new();
        public List<DeviceDirectoryViewModel> Devices { get; set; } = new();
        public int ActiveDeviceCount { get; set; }
        public int InactiveDeviceCount { get; set; }
    }

    public class QuotaDisplayViewModel
    {
        public int Id { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal Fees { get; set; }
        public bool IsActive { get; set; }
    }
}