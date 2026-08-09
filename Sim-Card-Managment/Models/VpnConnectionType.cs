using Sim_Card_Managment.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Management.Models
{
    public class VpnConnectionType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;  // e.g. Main, Backup, Mobile

        public int CreatedById { get; set; }
        // Navigation properties
        [ForeignKey(nameof(CreatedById))]
        public virtual User CreatedBy { get; set; }
        public virtual ICollection<VpnConnection> VpnConnections { get; set; } = new List<VpnConnection>();
    }
}
