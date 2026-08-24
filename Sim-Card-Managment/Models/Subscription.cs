using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        public int? EmpId { get; set; }
        public int? NonEmployeeId { get; set; }

        public int? SimId { get; set; }              
        public int? UsbId { get; set; }
        public int? QuotaId { get; set; }             

        [Required]
        public int ActionId { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Fees { get; set; } = 0;
        [StringLength(500)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(EmpId))]
        public virtual Employee? Employee { get; set; }
        [ForeignKey(nameof(NonEmployeeId))]
        public virtual NonEmployee? NonEmployee { get; set; }
        [ForeignKey(nameof(SimId))]
        public virtual Sim? Sim { get; set; }          
        [ForeignKey(nameof(UsbId))]
        public virtual Usb? Usb { get; set; }
        [ForeignKey(nameof(QuotaId))]
        public virtual Quota? Quota { get; set; }      
        [ForeignKey(nameof(ActionId))]
        public virtual DeviceAction Action { get; set; } = null!;
        [ForeignKey(nameof(CreatedBy))]
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ReceiverSignature? ReceiverSignature { get; set; }
        public virtual ICollection<DeviceTransfer> DeviceTransfers { get; set; } = new List<DeviceTransfer>();

    }
}
