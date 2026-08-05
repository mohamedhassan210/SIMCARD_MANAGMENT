using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Management.Models
{
    public class VpnConnectionType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;  // e.g. Main, Backup, Mobile

        // Navigation properties
        public virtual ICollection<VpnConnection> VpnConnections { get; set; } = new List<VpnConnection>();
    }
}
