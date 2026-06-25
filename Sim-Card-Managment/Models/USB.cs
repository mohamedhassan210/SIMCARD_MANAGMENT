using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class Usb
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Model { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";  // Active / Lost / Replaced / Returned

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<DeviceTransfer> DeviceTransfers { get; set; } = new List<DeviceTransfer>();
        public virtual ICollection<DeviceStatus> DeviceStatuses { get; set; } = new List<DeviceStatus>();
    }
}
