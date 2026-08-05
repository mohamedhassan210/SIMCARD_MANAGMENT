namespace Sim_Card_Managment.Viewmodel
{
    public class ServiceProviderDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
        public List<DeviceDirectoryViewModel> Devices { get; set; } = new();
        public int ActiveDeviceCount { get; set; }
        public int InactiveDeviceCount { get; set; }
    }
}