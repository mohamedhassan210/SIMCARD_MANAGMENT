using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Management.Models
{
    public class ServiceType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;  // e.g. ADSL, 3G/4G
        public bool HasPhoneNumber { get; set; } = false;

        // Navigation properties
        public virtual ICollection<InternetLine> InternetLines { get; set; } = new List<InternetLine>();
    }
}
