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
        // فاضل ال UserId وال object بتاعه اللي هيتعمل بيه ال foreign key
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
