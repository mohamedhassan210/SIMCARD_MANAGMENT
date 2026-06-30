using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class Quote
    {
        [Key]
        public Guid QuoteId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public decimal ExtraQuote { get; set; }
        public Guid? SimId { get; set; }
        public Guid? GoupId { get; set; }

        [ForeignKey(nameof(SimId))]
        public virtual SIM? SIM { get; set; }

        [ForeignKey(nameof(GoupId))]
        public virtual Group? Group { get; set; }
    }
}
