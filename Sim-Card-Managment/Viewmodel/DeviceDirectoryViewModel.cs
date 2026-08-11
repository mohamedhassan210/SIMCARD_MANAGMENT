using System;
namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceDirectoryViewModel
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // "SIM Card" or "USB Modem"
        public string? ExtraInfo { get; set; }
        public string? Detail => ExtraInfo;
        public string? Identifier { get; set; }
        public string Status { get; set; } = ;
        public bool IsActive { get; set; }
        public string? CurrentStatusType { get; set; } // e.g. "Lost", "Replaced", "Returned"; null if no incident ever logged

        public string? AssignedTo { get; set; }
        public string? AssignedToName => AssignedTo;
        public string? AssignedToType { get; set; }
        public string? ServiceProvider { get; set; }
        public string? ProviderName => ServiceProvider;
        public DateTime RegisteredAt { get; set; }
    }
}