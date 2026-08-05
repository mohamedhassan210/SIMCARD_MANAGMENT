using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Management.Models
{
    public class PaymentType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;  // e.g. Free Corporate, Not Free

        // Navigation properties
        public virtual ICollection<InternetLine> InternetLines { get; set; } = new List<InternetLine>();
    }
}
