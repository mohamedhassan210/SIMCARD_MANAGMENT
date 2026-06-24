using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class Group
    {
        [Key]
        public Guid GroupId { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
