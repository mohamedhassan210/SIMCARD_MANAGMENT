using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sim_Card_Management.Models;
namespace Sim_Card_Managment.Models
{
    public class DeviceStatus
    {
        [Key]
        public int Id { get; set; }
        public int? SimId { get; set; }
        public int? UsbId { get; set; }
        public int StatusTypeId { get; set; }
        [ForeignKey(nameof(StatusTypeId))]
        public DeviceStatusType StatusType { get; set; }   // Lost / Replaced / Returned / Damaged
        [Required]
        public DateTime StatusDate { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
        [Required]
        public int ReportedBy { get; set; }
        public int? ReplacedBySimId { get; set; }  // new SIM if replaced
        public int? ReplacedByUsbId { get; set; }  // new USB if replaced

        // NEW — a snapshot of who the device was assigned to at the moment this
        // status change was logged. Without this, "Assigned To" had to be computed
        // live from the device's currently-active Subscription, which meant every
        // past log row silently lost its assignee the moment that subscription
        // ended — this field freezes that value permanently at write time.
        [StringLength(200)]
        public string? AssignedToName { get; set; }

        // Navigation properties
        [ForeignKey(nameof(SimId))]
        public virtual Sim? Sim { get; set; }
        [ForeignKey(nameof(UsbId))]
        public virtual Usb? Usb { get; set; }
        [ForeignKey(nameof(ReportedBy))]
        public virtual User ReportedByUser { get; set; } = null!;
        [ForeignKey(nameof(ReplacedBySimId))]
        public virtual Sim? ReplacedBySim { get; set; }
        [ForeignKey(nameof(ReplacedByUsbId))]
        public virtual Usb? ReplacedByUsb { get; set; }
    }
}