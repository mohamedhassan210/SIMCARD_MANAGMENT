using System;

namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceStatusViewModel
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // "SIM Card" or "USB Modem"
        public string? Notes { get; set; } // "Details"

        // The status transition this record represents, e.g. "Unassigned -> Lost"
        public string OldStatus { get; set; } = "Unassigned";
        public string NewStatus { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public string Availability => IsActive ? "Active" : "Not Active";

        public string AssignedTo { get; set; } = "Unassigned";
        public string ReportedByUserName { get; set; } = string.Empty; // "status changed by user"

        public DateTime StatusDate { get; set; }
    }
}