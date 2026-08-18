using Sim_Card_Managment.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Management.Models
{
    public class InternetLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [Required]
        public int PaymentTypeId { get; set; }

        [Required]
        public int ServiceTypeId { get; set; }

        public int? SimId { get; set; }  // nullable - not every line has a SIM (e.g. ADSL)

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Bandwidth { get; set; }  // e.g. "UP TO 30 Mbps"
        public DateOnly? LastRenewalDate { get; set; }  

        public DateOnly? NextRenewalDate { get; set; }  

        [Column(TypeName = "decimal(10,2)")]
        public decimal? QuotaGB { get; set; }  // monthly quota, filled when ServiceType = 3G/4G

        [Required]
        public bool Status { get; set; } = true;  // true = UP, false = Down

        [StringLength(500)]
        public string? Notes { get; set; }
        public int? RenewaltypeId { get; set; }
        [ForeignKey(nameof(RenewaltypeId))]
        public RenewalType? RenewalType { get; set; }
        public int CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        public virtual User CreatedBy { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; } = null!;

        [ForeignKey(nameof(ServiceProviderId))]
        public virtual Sim_Card_Managment.Models.ServiceProvider ServiceProvider { get; set; } = null!;

        [ForeignKey(nameof(PaymentTypeId))]
        public virtual PaymentType PaymentType { get; set; } = null!;

        [ForeignKey(nameof(ServiceTypeId))]
        public virtual ServiceType ServiceType { get; set; } = null!;

        [ForeignKey(nameof(SimId))]
        public virtual Sim? Sim { get; set; }
    }
}
