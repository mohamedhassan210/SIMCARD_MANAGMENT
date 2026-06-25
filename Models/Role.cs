using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class Role
    {
        [Key]
        public Guid RoleId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Permessions { get; set; }

    }
}
