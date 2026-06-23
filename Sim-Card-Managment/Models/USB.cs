using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class USB
    {
        [Key]
        public Guid SerialNumber { get; set; }
    }
}
