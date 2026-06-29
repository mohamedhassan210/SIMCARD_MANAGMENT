using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.ViewModels.Subscription
{
    // ─────────────────────────────────────────────
    // Used in: Index page (table rows)
    // ─────────────────────────────────────────────
    public class SubscriptionIndexVM
    {
        public Guid Id { get; set; }

        // Subscriber
        public string SubscriberName { get; set; } = string.Empty;   // Employee or NonEmployee
        public string SubscriberType { get; set; } = string.Empty;   // "Employee" | "Non-Employee"

        // Devices
        public string SimNumber { get; set; } = string.Empty;
        public string? UsbSerialNumber { get; set; }

        // Plan / Action
        public string QuotaName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;

        // Dates
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status => EndDate == null
            ? "Active"
            : EndDate < DateTime.Now ? "Expired" : "Scheduled End";

        public DateTime CreatedDate { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────
    // Used in: Create form
    // ─────────────────────────────────────────────
    public class SubscriptionCreateVM
    {
        // Subscriber — one of these two must be set
        public Guid? EmpId { get; set; }
        public Guid? NonEmployeeId { get; set; }

        [Required(ErrorMessage = "SIM card is required.")]
        [Display(Name = "SIM Card")]
        public Guid SimId { get; set; }

        [Display(Name = "USB Device")]
        public Guid? UsbId { get; set; }

        [Required(ErrorMessage = "Quota plan is required.")]
        [Display(Name = "Quota Plan")]
        public Guid QuotaId { get; set; }

        [Required(ErrorMessage = "Action is required.")]
        [Display(Name = "Device Action")]
        public Guid ActionId { get; set; }

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
    public class SubscriptionEditVM
    {
        [Required]
        public Guid Id { get; set; }

        public Guid? EmpId { get; set; }
        public Guid? NonEmployeeId { get; set; }

        [Required(ErrorMessage = "SIM card is required.")]
        [Display(Name = "SIM Card")]
        public Guid SimId { get; set; }

        [Display(Name = "USB Device")]
        public Guid? UsbId { get; set; }

        [Required(ErrorMessage = "Quota plan is required.")]
        [Display(Name = "Quota Plan")]
        public Guid QuotaId { get; set; }

        [Required(ErrorMessage = "Action is required.")]
        [Display(Name = "Device Action")]
        public Guid ActionId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Read-only metadata shown on the edit page
        public DateTime CreatedDate { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;

        // Drop-down source lists
        public List<DropDownItem> Employees { get; set; } = new();
        public List<DropDownItem> NonEmployees { get; set; } = new();
        public List<DropDownItem> SimCards { get; set; } = new();
        public List<DropDownItem> UsbDevices { get; set; } = new();
        public List<DropDownItem> Quotas { get; set; } = new();
        public List<DropDownItem> Actions { get; set; } = new();
    }

    // ─────────────────────────────────────────────
    // Used in: Details / read-only view
    // ─────────────────────────────────────────────
    public class SubscriptionDetailsVM
    {
        public Guid Id { get; set; }

        // Subscriber
        public string SubscriberName { get; set; } = string.Empty;
        public string SubscriberType { get; set; } = string.Empty;
        public Guid? EmpId { get; set; }
        public Guid? NonEmployeeId { get; set; }

        // Devices
        public string SimNumber { get; set; } = string.Empty;
        public Guid SimId { get; set; }
        public string? UsbSerialNumber { get; set; }
        public Guid? UsbId { get; set; }

        // Plan / Action
        public string QuotaName { get; set; } = string.Empty;
        public Guid QuotaId { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public Guid ActionId { get; set; }

        // Dates & Status
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status => EndDate == null
            ? "Active"
            : EndDate < DateTime.Now ? "Expired" : "Scheduled End";

        // Audit
        public DateTime CreatedDate { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;

        // Notes
        public string? Notes { get; set; }

        // Related collections summary
        public int TransferCount { get; set; }
        public bool HasReceiverSignature { get; set; }
    }

    // ─────────────────────────────────────────────
    // Used in: Delete confirmation page
    // ─────────────────────────────────────────────
    public class SubscriptionDeleteVM
    {
        public Guid Id { get; set; }
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
        public Guid Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}