using System;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class SubscriptionCreateViewModel
    {
        public Guid SelectedEmployeeId { get; set; }
        public string DeviceType { get; set; } = "SIM";

        // MUST be Guid? so MVC doesn't mark model invalid when submitting in USB mode
        public Guid? SelectedSimId { get; set; }
        public Guid? SelectedQuotaId { get; set; }
        public Guid? SelectedUsbId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;
        public int ContractDurationYears { get; set; } = 1;
        public bool AgreedToTerms { get; set; }
    }
}