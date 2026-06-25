using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class DeviceTransfer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FromSubscriptionId { get; set; }

        [Required]
        public Guid ToEmpId { get; set; }

        public Guid? SimId { get; set; }

        public Guid? UsbId { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }

        [StringLength(100)]
        public string? Reason { get; set; }  // Lost / Replaced / Reassigned

        [Required]
        public Guid CreatedBy { get; set; }

        public Guid? NewSubscriptionId { get; set; }  // subscription created after transfer

        // Navigation properties
        [ForeignKey(nameof(FromSubscriptionId))]
        public virtual Subscription FromSubscription { get; set; } = null!;

        [ForeignKey(nameof(NewSubscriptionId))]
        public virtual Subscription? NewSubscription { get; set; }

        [ForeignKey(nameof(ToEmpId))]
        public virtual Employee ToEmployee { get; set; } = null!;

        [ForeignKey(nameof(SimId))]
        public virtual Sim? Sim { get; set; }

        [ForeignKey(nameof(UsbId))]
        public virtual Usb? Usb { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
