using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class DeviceAction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;  // e.g. Assign, Replace, Return

        [StringLength(20)]
        public string ActionStatus { get; set; } = "Active";  // Active / Inactive

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
