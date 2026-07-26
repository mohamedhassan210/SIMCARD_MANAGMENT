using System;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class SubscriptionCreateViewModel
    {
        // 1. Changed to Guid? so .HasValue and .Value work properly
        public Guid? SelectedEmployeeId { get; set; }

        // 2. Added IsNonEmployee property for UI toggle mapping
        public bool IsNonEmployee { get; set; } = false;

        public string DeviceType { get; set; } = "SIM";

        public Guid? SelectedSimId { get; set; }
        public Guid? SelectedQuotaId { get; set; }
        public Guid? SelectedUsbId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;
        public int ContractDurationYears { get; set; } = 1;
        public bool AgreedToTerms { get; set; }
    }
}