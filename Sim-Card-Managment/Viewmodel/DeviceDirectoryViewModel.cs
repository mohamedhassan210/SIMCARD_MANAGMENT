namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceDirectoryViewModel
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // "SIM" | "USB"
        public string? ExtraInfo { get; set; } // NetworkType or Model
        public string? Identifier { get; set; } // PhoneNumber for SIM, null for USB
        public string Status { get; set; } = string.Empty;
        public string? AssignedTo { get; set; } // Current assignee name
        public string? ServiceProvider { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}