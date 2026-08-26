using System;
using System.Collections.Generic;

namespace Sim_Card_Managment.ViewModels
{
    public class DocumentDetailsViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentTypeName { get; set; }
        public DateTime ActionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; }

        public int SimCount => Sims?.Count ?? 0;
        public int UsbCount => Usbs?.Count ?? 0;

        public List<SimDetailViewModel> Sims { get; set; } = new List<SimDetailViewModel>();
        public List<UsbDetailViewModel> Usbs { get; set; } = new List<UsbDetailViewModel>();
    }

    public class SimDetailViewModel
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string? ProviderName { get; set; }
        public string? AssignedTo { get; set; }
    }

    public class UsbDetailViewModel
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string? ProviderName { get; set; }
        public string? AssignedTo { get; set; }
    }
}