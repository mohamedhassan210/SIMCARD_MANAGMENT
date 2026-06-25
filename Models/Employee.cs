using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class Employee
    {
        [Key]
        public Guid EmpId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public string Position { get; set; }
        public string NationalId { get; set; }
    }
}
