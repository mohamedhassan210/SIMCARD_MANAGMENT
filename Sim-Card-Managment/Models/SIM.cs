using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Sim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;  // ICCID

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(10)]
        public string? NetworkType { get; set; }  // 4G / 5G

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Fees { get; set; }

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
