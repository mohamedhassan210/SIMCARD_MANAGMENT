using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Status
    {
        [Key]
        public Guid StatusId { get; set; }
        [Required]
        public string StatusName { get; set; }
        public Guid ActionId { get; set; }
        [ForeignKey(nameof(ActionId))]
        public virtual Action? action { get; set; }
    }
}
