using Sim_Card_Managment.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Management.Models
{
    public class DeviceSerialOperation
    {
        [Key]
        public int Id { get; set; }
        public int SimId { get; set; }
        public string OldSerialNumber { get; set; }
        public string NewSerialNumber { get; set; }
        public bool NetworkTypeChange { get; set; } = false;
        [ForeignKey(nameof(SimId))]
        public virtual Sim SIM { get; set; }
        public DateTime OperationDate { get; set; }
        public int CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        public virtual User CreatedBy { get; set; }
    }
}
