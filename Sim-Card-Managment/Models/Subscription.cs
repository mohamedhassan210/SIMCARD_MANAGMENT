using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? EmpId { get; set; }          // nullable if assigned to NonEmployee

        public Guid? NonEmployeeId { get; set; }  // nullable if assigned to Employee

        [Required]
        public Guid SimId { get; set; }

        public Guid? UsbId { get; set; }

        [Required]
        public Guid QuotaId { get; set; }

        [Required]
        public Guid ActionId { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }  // null = open ended

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(EmpId))]
        public virtual Employee? Employee { get; set; }

        [ForeignKey(nameof(NonEmployeeId))]
        public virtual NonEmployee? NonEmployee { get; set; }

        [ForeignKey(nameof(SimId))]
        public virtual Sim Sim { get; set; } = null!;

        [ForeignKey(nameof(UsbId))]
        public virtual Usb? Usb { get; set; }

        [ForeignKey(nameof(QuotaId))]
        public virtual Quota Quota { get; set; } = null!;

        [ForeignKey(nameof(ActionId))]
        public virtual DeviceAction Action { get; set; } = null!;

        [ForeignKey(nameof(CreatedBy))]
        public virtual User CreatedByUser { get; set; } = null!;

        public virtual ReceiverSignature? ReceiverSignature { get; set; }
        public virtual ICollection<DeviceTransfer> DeviceTransfers { get; set; } = new List<DeviceTransfer>();
    }
}
