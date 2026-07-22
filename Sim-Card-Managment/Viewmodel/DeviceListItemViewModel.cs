using System;

namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceListItemViewModel
    {
        public Guid Id { get; set; }
        public string DeviceType { get; set; } = string.Empty; // "SIM" or "USB"
        public string SerialNumber { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty; // Phone Number for SIM, Model for USB
        public string Status { get; set; } = string.Empty; // "Active", "Available", "Inactive"
        public string ProviderName { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string AssignedToType { get; set; } = string.Empty; // "Employee" or "Non-Employee"
        public DateTime RegisteredAt { get; set; }
    }
}