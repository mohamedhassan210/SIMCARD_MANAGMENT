using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Quota
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseAmount { get; set; }  // GB per month

        [Column(TypeName = "decimal(10,2)")]
        public decimal ExtraAmount { get; set; } = 0;  // Extra GB granted
        [Column(TypeName = "decimal(10,2)")]
        public decimal Fees { get; set; } 

        public bool IsActive { get; set; }
        public int ServiceProviderId { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }
        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
       
    }
}
