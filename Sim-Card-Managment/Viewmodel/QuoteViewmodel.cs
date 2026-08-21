using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sim_Card_Managment.Viewmodel
{
    public class QuotaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Base Amount is required")]    
        [Display(Name = "Base Amount (GB)")]
        public decimal BaseAmount { get; set; }

        
        [Display(Name = "Extra Amount (GB)")]
        public decimal ExtraAmount { get; set; }

        [Required(ErrorMessage = "Fees is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Fees cannot be negative")]
        [Display(Name = "Fees")]
        public decimal Fees { get; set; }

        [Required(ErrorMessage = "Service Provider is required")]
        [Display(Name = "Service Provider")]
        public int ServiceProviderId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Populated by the controller for the dropdown; not itself posted back.
        public SelectList? ServiceProviders { get; set; }

        // Convenience for Index rows — not bound on Create/Edit posts.
        [Display(Name = "Provider")]
        public string? ServiceProviderName { get; set; }
    }
}