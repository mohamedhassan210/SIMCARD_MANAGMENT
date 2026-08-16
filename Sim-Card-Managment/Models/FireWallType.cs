using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Management.Models
{
    public class FireWallType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}
