using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class ReceiverSignature
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SubscriptionId { get; set; }

        [Required]
        public Guid SignedBy { get; set; }

        [Required]
        public DateTime SignedDate { get; set; }

        [StringLength(20)]
        public string? SignatureType { get; set; }  // Physical / Digital

        public string? SignatureData { get; set; }  // base64 or file path

        // Navigation properties
        [ForeignKey(nameof(SubscriptionId))]
        public virtual Subscription Subscription { get; set; } = null!;

        [ForeignKey(nameof(SignedBy))]
        public virtual User SignedByUser { get; set; } = null!;
    }
}
