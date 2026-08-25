using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceStatusCreateViewModel
    {
        public int? SimId { get; set; }
        public int? UsbId { get; set; }
        public int? StatusTypeId { get; set; }
        public string? Notes { get; set; }
        public int? ReplacedBySimId { get; set; }
        public int? ReplacedByUsbId { get; set; }

        public List<DeviceOptionViewModel> Sims { get; set; } = new();
        public List<DeviceOptionViewModel> Usbs { get; set; } = new();

        public SelectList? StatusTypes { get; set; }
    }

    public class DeviceOptionViewModel
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}