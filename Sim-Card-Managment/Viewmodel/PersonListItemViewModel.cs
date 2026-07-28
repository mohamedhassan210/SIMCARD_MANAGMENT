using System;

namespace Sim_Card_Managment.ViewModels
{
    public class PersonListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PersonType { get; set; } = "Employee"; // "Employee" or "Non-Employee"
        public string Identifier { get; set; } = string.Empty; // NationalID or ContactInfo
        public string? ExtraInfo { get; set; } // Position or Type (Contractor, Visitor, etc.)
        public int ActiveSimOnlyCount { get; set; }
        public int ActiveUsbCount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
    }
}