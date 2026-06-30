using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Quota
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseAmount { get; set; }  // GB per month

        [Column(TypeName = "decimal(10,2)")]
        public decimal ExtraAmount { get; set; } = 0;  // Extra GB granted

        [StringLength(20)]
        public string? Period { get; set; }  // Monthly / Quarterly

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
