using Sim_Card_Managment.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Management.Models
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;  // e.g. HQ, Rosana, Amwaj

        public bool IsActive { get; set; } = true;

        public bool? VpnOverInternetStatus { get; set; }  // true = OK
        public string? SiteCode{ get; set; } // HR code
        public string? BranchCode { get; set; } // Retail code
        public string? Note { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CreatedById { get; set; }
        // Navigation properties
        [ForeignKey(nameof(CreatedById))]
        public virtual User CreatedBy { get; set; }
        public virtual ICollection<InternetLine> InternetLines { get; set; } = new List<InternetLine>();
        public virtual ICollection<VpnConnection> VpnConnections { get; set; } = new List<VpnConnection>();
        public virtual ICollection<FireWallType> FireWallTypes { get; set; }
    }
}
