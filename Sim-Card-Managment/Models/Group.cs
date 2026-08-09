using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedById { get; set; }
        public bool IsActive { get; set; }
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
        [ForeignKey(nameof(CreatedById))]
        public virtual User? CreatedBy { get; set; }
    }
}
