using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class NonEmployee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ContactInfo { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }  // e.g. Contractor, Visitor
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
