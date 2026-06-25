using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Action
    {
        [Key]
        public Guid ActionId { get; set; }
        [Required]
        public string ActionName { get; set; }
        public string Status { get; set; }
    }
}
