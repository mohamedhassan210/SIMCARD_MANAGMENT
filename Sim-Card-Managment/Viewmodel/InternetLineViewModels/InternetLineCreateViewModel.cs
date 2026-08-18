using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class InternetLineCreateViewModel
    {
        [Required]
        public int BranchId { get; set; }
        [Required]
        public int ServiceProviderId { get; set; }
        [Required]
        public int PaymentTypeId { get; set; }
        [Required]
        public int ServiceTypeId { get; set; }
        public int? SimId { get; set; }       
        [StringLength(11, ErrorMessage = "Phone number cannot exceed 11 digits.")]
        [RegularExpression(@"^\d{0,11}$", ErrorMessage = "Phone number must contain only digits, up to 11.")]
        public string? PhoneNumber { get; set; }
        [StringLength(100)]
        public string? Bandwidth { get; set; }

        [Required]
        public int RenewaltypeId { get; set; }

        // Optional - if not supplied, the repo defaults this to today.
        // NextRenewalDate is always calculated server-side from
        // LastRenewalDate + RenewalType.DurationInMonths.
        [DataType(DataType.Date)]
        public DateOnly? LastRenewalDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? NextRenewalDate { get; set; }

        // Id -> DurationInMonths, populated by the controller so the view's JS
        // can auto-calculate NextRenewalDate without a server round trip.
        public Dictionary<int, int> RenewalTypeDurations { get; set; } = new();

        public decimal? QuotaGB { get; set; }
        public bool Status { get; set; } = true;
        [StringLength(500)]
        public string? Notes { get; set; }
        public int CreatedById { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> Branches { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ServiceProviders { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ServiceTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> RenewalTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Sims { get; set; } = new List<SelectListItem>();
    }
}