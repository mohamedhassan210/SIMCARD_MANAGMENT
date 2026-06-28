using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Subscription
    {
        [Key]
        public Guid SubId { get; set; }
        public Guid? EmpId { get; set; }
        public Guid? NotEmployeeId { get; set; }
        public Guid? SIMId { get; set; }
        public Guid? USBId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ActionId {  get; set; }

        [ForeignKey(nameof(ActionId))]
        public virtual Action? Action { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
        [ForeignKey(nameof(EmpId))]
        public virtual Employee? Employee { get; set; }
        [ForeignKey(nameof(NotEmployeeId))]
        public virtual NonEmployee? NonEmployee { get; set; }
        [ForeignKey(nameof(SIMId))]
        public virtual SIM? Sim { get; set; }
        [ForeignKey(nameof(USBId))]
        public virtual USB? usb { get; set; }
    }
}
