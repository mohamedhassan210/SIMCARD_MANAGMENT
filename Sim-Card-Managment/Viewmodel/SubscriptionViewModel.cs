using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.ViewModels.Subscription
{
    // ─────────────────────────────────────────────
    // Used in: Index page (table rows)
    // ─────────────────────────────────────────────
    public class SubscriptionIndexVM
    {
        public int Id { get; set; }
        public string SubscriberName { get; set; } = string.Empty;
        public string SubscriberType { get; set; } = string.Empty;
        public string? SubscriberIdentifier { get; set; }   // EmpCode or ContactInfo

        public bool HasSim { get; set; }
        public string? SimSerialNumber { get; set; }
        public bool HasUsb { get; set; }
        public string? UsbSerialNumber { get; set; }

        public string QuotaName { get; set; } = string.Empty;
        public decimal Fees { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status => EndDate == null
            ? "Active"
            : EndDate < DateTime.Now ? "Expired" : "Scheduled End";

        public string CreatedByUserName { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────
    // Used in: Create form
    // ─────────────────────────────────────────────
    public class SubscriptionCreateVM
    {
        // Subscriber — one of these two must be set
        public int? EmpId { get; set; }
        public int? NonEmployeeId { get; set; }

        [Required(ErrorMessage = "SIM card is required.")]
        [Display(Name = "SIM Card")]
        public int SimId { get; set; }

        [Display(Name = "USB Device")]
        public int? UsbId { get; set; }

        [Required(ErrorMessage = "Quota plan is required.")]
        [Display(Name = "Quota Plan")]
        public int QuotaId { get; set; }

        [Required(ErrorMessage = "Action is required.")]
        [Display(Name = "Device Action")]
        public int ActionId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // ── Drop-down source lists (populated by controller) ──
        public List<DropDownItem> Employees { get; set; } = new();
        public List<DropDownItem> NonEmployees { get; set; } = new();
        public List<DropDownItem> SimCards { get; set; } = new();
        public List<DropDownItem> UsbDevices { get; set; } = new();
        public List<DropDownItem> Quotas { get; set; } = new();
        public List<DropDownItem> Actions { get; set; } = new();
    }

    // ─────────────────────────────────────────────
    // Used in: Edit form
    // ─────────────────────────────────────────────
    public class SubscriptionEditViewModel
    {
        public int Id { get; set; }

        public string SubscriberName { get; set; } = string.Empty;
        public string SubscriberType { get; set; } = string.Empty;

        public int? SelectedSimId { get; set; }
        public string? CurrentSimSerial { get; set; }
        public int? CurrentSimProviderId { get; set; }
        public string? CurrentSimProviderName { get; set; }   // NEW — for the current-SIM card's header/logo
        public string? CurrentSimNetworkType { get; set; }    // NEW — for the current-SIM card's title line
        public string? CurrentSimPhoneNumber { get; set; }    // NEW — for the current-SIM card's title line

        public int? SelectedUsbId { get; set; }
        public string? CurrentUsbSerial { get; set; }
        public string? CurrentUsbModel { get; set; }           // NEW — for the current-USB card's title line
        public string? CurrentUsbProviderName { get; set; }    // NEW — for the current-USB card's subtitle line

        public int? SelectedQuotaId { get; set; }
        public string? CurrentQuotaDisplay { get; set; }
        public decimal? CurrentQuotaBaseAmount { get; set; }   // NEW — for the current-quota card's title line
        public decimal? CurrentQuotaExtraAmount { get; set; }  // NEW — for the current-quota card's subtitle line
        public decimal? CurrentQuotaFee { get; set; }          // NEW — for the current-quota card's subtitle line

        public decimal Fees { get; set; }
        public decimal OriginalFees { get; set; }   // NEW — lets "reselect current" restore the original fee
    }

    // ─────────────────────────────────────────────
    // Used in: Details / read-only view
    // ─────────────────────────────────────────────
    public class SubscriptionDetailsVM
    {
        public int Id { get; set; }

        public string SubscriberName { get; set; } = string.Empty;
        public string SubscriberType { get; set; } = string.Empty;
        public int? EmpId { get; set; }
        public int? NonEmployeeId { get; set; }

        public int? SimId { get; set; }
        public string? SimNumber { get; set; }
        public string? SimPhoneNumber { get; set; }
        public string? SimNetworkType { get; set; }
        public string? SimProviderName { get; set; }
        public bool SimIsActive { get; set; }

        public int? UsbId { get; set; }
        public string? UsbSerialNumber { get; set; }
        public string? UsbModel { get; set; }
        public string? UsbProviderName { get; set; }
        public bool UsbIsActive { get; set; }

        public string QuotaName { get; set; } = string.Empty;
        public int? QuotaId { get; set; }
        public decimal Fees { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status => EndDate == null
            ? "Active"
            : EndDate < DateTime.Now ? "Expired" : "Scheduled End";

        public DateTime CreatedDate { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    // ─────────────────────────────────────────────
    // Used in: Delete confirmation page
    // ─────────────────────────────────────────────
    public class SubscriptionDeleteVM
    {
        public int Id { get; set; }
        public string SubscriberName { get; set; } = string.Empty;
        public string SimNumber { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    // ─────────────────────────────────────────────
    // Shared helper — generic drop-down item
    // ─────────────────────────────────────────────
    public class DropDownItem
    {
        public int Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}