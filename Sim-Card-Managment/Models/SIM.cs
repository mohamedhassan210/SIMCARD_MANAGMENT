using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class SIM
    {
        [Key]
        public Guid SimId { get; set; }
        public string Number { get; set; }
        public double Fees { get; set; }

    }
}
