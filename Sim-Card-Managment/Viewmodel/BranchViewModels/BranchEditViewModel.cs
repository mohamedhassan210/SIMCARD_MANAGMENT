using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class BranchEditViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool? VpnOverInternetStatus { get; set; }

        [StringLength(50)]
        [Display(Name = "Site Code (HR)")]
        public string? SiteCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Branch Code (Retail)")]
        public string? BranchCode { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        // Firewall types currently checked for this branch.
        public List<int> SelectedFireWallTypeIds { get; set; } = new();

        // Populated by the controller for the checkbox list; not itself posted back.
        public IEnumerable<SelectListItem>? FireWallTypes { get; set; }
    }
}