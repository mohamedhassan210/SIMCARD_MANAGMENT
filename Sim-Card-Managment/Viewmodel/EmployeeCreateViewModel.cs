using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(14, MinimumLength = 14)]
        public string NationalID { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EmpCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Position { get; set; }
    }
}