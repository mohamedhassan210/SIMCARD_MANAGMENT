using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class NonEmployee
    {
        [Key]
        public Guid NotEmployeeId { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
