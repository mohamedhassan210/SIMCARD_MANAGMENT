using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ActionType { get; set; } = string.Empty;  // INSERT / UPDATE / DELETE

        public Guid RecordId { get; set; }

        [Required]
        public Guid PerformedBy { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.Now;

        public string? OldValues { get; set; }  // JSON snapshot before

        public string? NewValues { get; set; }  // JSON snapshot after

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(100)]
        public string? Module { get; set; }  // UI module name

        // Navigation properties
        [ForeignKey(nameof(PerformedBy))]
        public virtual User PerformedByUser { get; set; } = null!;
    }
}
