using System.ComponentModel.DataAnnotations;
using Sim_Card_Managment.Models;

namespace Sim_Card_Management.Models
{
    public class DeviceStatusType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<DeviceStatus> DeviceStatuses { get; set; } = new List<DeviceStatus>();
    }
}
