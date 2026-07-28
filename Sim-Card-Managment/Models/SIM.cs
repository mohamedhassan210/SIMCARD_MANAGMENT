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
        public string SerialNumber { get; set; } = string.Empty;  // ICCID    //////textbox

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;     //////textbox

        [StringLength(10)]
        public string? NetworkType { get; set; }  // 4G / 5G    //////drop

           

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";  // Active / Lost / Replaced / Returned   //////drop

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<DeviceTransfer> DeviceTransfers { get; set; } = new List<DeviceTransfer>();
        public virtual ICollection<DeviceStatus> DeviceStatuses { get; set; } = new List<DeviceStatus>();
        public virtual ICollection<Serial> Serials { get; set; } = new List<Serial>();



        public int ServiceProviderId { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }      //////drop
    }
}
