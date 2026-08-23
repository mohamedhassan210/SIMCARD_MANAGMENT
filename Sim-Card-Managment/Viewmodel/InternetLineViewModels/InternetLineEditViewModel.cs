using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class InternetLineEditViewModel
    {
        public int Id { get; set; }
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

        // Display-only, used to render the currently-assigned SIM as a
        // pinned card on the Edit view. Never posted back on save.
        public string? SimSerialNumber { get; set; }
        public string? SimProviderName { get; set; }

        [StringLength(100)]
        public string? Bandwidth { get; set; }

        [Required]
        public int? RenewaltypeId { get; set; }

        // Editable directly here so an admin can correct the cycle by hand;
        // if left as-is the repo keeps whatever was already saved.
        [DataType(DataType.Date)]
        public DateOnly? LastRenewalDate { get; set; }
        [DataType(DataType.Date)]
        public DateOnly? NextRenewalDate { get; set; }

        public Dictionary<int, int> RenewalTypeDurations { get; set; } = new();
        public Dictionary<int, bool> ServiceTypeHasPhoneNumber { get; set; } = new();

        public decimal? QuotaGB { get; set; }
        public bool Status { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> Branches { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ServiceProviders { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ServiceTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> RenewalTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Sims { get; set; } = new List<SelectListItem>();
    }
}