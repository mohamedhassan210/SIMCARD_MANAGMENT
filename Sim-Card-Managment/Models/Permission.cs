using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ActionName { get; set; } = string.Empty;  // e.g. Create, Read, Update, Delete

        [Required]
        [StringLength(100)]
        public string ControllerName { get; set; } = string.Empty;  // e.g. SubscriptionController

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    }
}
