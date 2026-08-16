using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class BranchCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool? VpnOverInternetStatus { get; set; }
        public int CreatedById { get; set; }

        [StringLength(50)]
        [Display(Name = "Site Code (HR)")]
        public string? SiteCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Branch Code (Retail)")]
        public string? BranchCode { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        // Firewall types the user checked. Only meaningful when
        // VpnOverInternetStatus == true, but we don't reject extra
        // selections server-side — the UI just hides the section.
        public List<int> SelectedFireWallTypeIds { get; set; } = new();

        // Populated by the controller for the checkbox list; not itself posted back.
        public IEnumerable<SelectListItem>? FireWallTypes { get; set; }
    }
}