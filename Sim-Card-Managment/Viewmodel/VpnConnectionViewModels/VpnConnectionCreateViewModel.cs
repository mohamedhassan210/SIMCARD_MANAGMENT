using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class VpnConnectionCreateViewModel
    {
        [Required]
        public int BranchId { get; set; }
        [Required]
        public int ConnectionTypeId { get; set; }
        [Required]
        public int ServiceProviderId { get; set; }
        [StringLength(50)]
        public string? NID { get; set; }
        [StringLength(50)]
        public string? LineSpeed { get; set; }
        public bool? Status { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
        public int CreatedById { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> Branches { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ConnectionTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ServiceProviders { get; set; } = new List<SelectListItem>();
    }
}