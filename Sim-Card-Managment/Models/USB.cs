using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class USB
    {
        [Key]
        public Guid USBId { get; set; }
        public string SerialNumber { get; set; }
    }
}
