using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class NonEmployee
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ContactInfo { get; set; }

        [StringLength(100)]
        public string? Type { get; set; }  // e.g. Contractor, Visitor

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
