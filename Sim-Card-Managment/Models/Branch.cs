using System.ComponentModel.DataAnnotations;

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

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<InternetLine> InternetLines { get; set; } = new List<InternetLine>();
        public virtual ICollection<VpnConnection> VpnConnections { get; set; } = new List<VpnConnection>();
    }
}
