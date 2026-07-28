using System;

namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceDirectoryViewModel
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // "SIM" or "USB"

        public string? ExtraInfo { get; set; } // NetworkType for SIM, Model for USB
        public string? Detail => ExtraInfo;    // Alias for View compatibility

        public string? Identifier { get; set; } // Phone Number for SIM

        public string Status { get; set; } = string.Empty; // "Active", "Available", "Inactive"

        public string? AssignedTo { get; set; } // Employee Name or NonEmployee Name
        public string? AssignedToName => AssignedTo; // Alias for View compatibility
        public string? AssignedToType { get; set; }  // "Employee" or "NonEmployee"

        public string? ServiceProvider { get; set; }
        public string? ProviderName => ServiceProvider; // Alias for View compatibility

        public DateTime RegisteredAt { get; set; }
    }
}