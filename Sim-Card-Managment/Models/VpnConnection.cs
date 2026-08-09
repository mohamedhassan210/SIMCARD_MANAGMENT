using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Sim_Card_Managment.Models;

namespace Sim_Card_Management.Models
{
    public class VpnConnection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int ConnectionTypeId { get; set; }  // Main / Backup / Mobile

        [Required]
        public int ServiceProviderId { get; set; }

        [StringLength(50)]
        public string? NID { get; set; }  // e.g. "10.1.0.0/23" - typically filled for Main

        [StringLength(50)]
        public string? LineSpeed { get; set; }

        public bool? Status { get; set; }  // true = online, false = offline

        [StringLength(500)]
        public string? Notes { get; set; }

        public int CreatedById { get; set; }
        // Navigation properties
        [ForeignKey(nameof(CreatedById))]
        public virtual User CreatedBy { get; set; }
        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; } = null!;

        [ForeignKey(nameof(ConnectionTypeId))]
        public virtual VpnConnectionType ConnectionType { get; set; } = null!;

        [ForeignKey(nameof(ServiceProviderId))]
        public virtual Sim_Card_Managment.Models.ServiceProvider ServiceProvider { get; set; } = null!;
    }
}
