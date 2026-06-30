using System;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class QuotaViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Base Amount is required")]
        [Range(0.1, 1000, ErrorMessage = "Base Amount must be between 0.1 and 1000 GB")]
        [Display(Name = "Base Amount (GB)")]
        public decimal BaseAmount { get; set; }

        [Display(Name = "Extra Amount (GB)")]
        public decimal ExtraAmount { get; set; }

        [Required(ErrorMessage = "Period description is required")]
        [StringLength(50, ErrorMessage = "Period cannot exceed 50 characters")]
        public string Period { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Valid From")]
        public DateTime ValidFrom { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Valid To")]
        public DateTime ValidTo { get; set; } = DateTime.Now.AddMonths(1);
    }
}