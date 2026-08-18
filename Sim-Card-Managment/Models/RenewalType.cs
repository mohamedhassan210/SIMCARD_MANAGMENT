using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Management.Models
{
    public class RenewalType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int DurationInMonths { get; set; }
        public ICollection<InternetLine> InternetLines { get; set; } = new List<InternetLine>();
    }
}
