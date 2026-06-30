using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class RoleGroup
    {
        public Guid RoleGroupId { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? GroupId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role? Role { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual Group? Group { get; set; }
    }
}
