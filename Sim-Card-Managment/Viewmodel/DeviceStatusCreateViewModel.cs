using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceStatusCreateViewModel
    {
        // Exactly one of these two should be set — the device this status report is for
        public int? SimId { get; set; }
        public int? UsbId { get; set; }

        public int StatusTypeId { get; set; } // Lost / Replaced / Returned / Damaged

        public string? Notes { get; set; }

        // Optional — only relevant when StatusType is "Replaced"
        public int? ReplacedBySimId { get; set; }
        public int? ReplacedByUsbId { get; set; }

        public SelectList? Sims { get; set; }
        public SelectList? Usbs { get; set; }
        public SelectList? StatusTypes { get; set; }
        public SelectList? ReplacementSims { get; set; }
        public SelectList? ReplacementUsbs { get; set; }
    }
}