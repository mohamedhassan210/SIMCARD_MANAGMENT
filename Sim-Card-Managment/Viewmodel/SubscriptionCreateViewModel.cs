using System;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class SubscriptionCreateViewModel
    {
        // 1. Changed to int? so .HasValue and .Value work properly
        public int? SelectedEmployeeId { get; set; }

        // 2. Added IsNonEmployee property for UI toggle mapping
        public bool IsNonEmployee { get; set; } = false;

        public string DeviceType { get; set; } = "SIM";

        public int? SelectedSimId { get; set; }
        public int? SelectedQuotaId { get; set; }
        public int? SelectedUsbId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;
        public int ContractDurationYears { get; set; } = 1;
        public bool AgreedToTerms { get; set; }
    }
}