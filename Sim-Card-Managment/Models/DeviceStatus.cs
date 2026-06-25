using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class DeviceStatus
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? SimId { get; set; }

        public Guid? UsbId { get; set; }

        [Required]
        [StringLength(30)]
        public string StatusType { get; set; } = string.Empty;  // Lost / Replaced / Returned / Damaged

        [Required]
        public DateTime StatusDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public Guid ReportedBy { get; set; }

        public Guid? ReplacedBySimId { get; set; }  // new SIM if replaced

        public Guid? ReplacedByUsbId { get; set; }  // new USB if replaced

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
