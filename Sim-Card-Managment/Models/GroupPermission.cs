using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class GroupPermission
    {
        [Required]
        public int GroupId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(GroupId))]
        public virtual Group Group { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; set; } = null!;
    }
}
