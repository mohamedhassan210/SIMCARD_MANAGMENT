using System.ComponentModel.DataAnnotations;
using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        public int? DocumenttypeId { get; set; }
        public DocumentType DocumentType { get; set; }
        public DateTime ActionDate { get; set; }
        [MaxLength(500)]
        public string Notes { get; set; }
        [MaxLength(50)]
        public string SignatureType { get; set; }
        [MaxLength(50)]
        public string SignatureData { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }

        public User CreatedBy { get; set; }

        [Required(ErrorMessage = "يجب إدخال سيريال واحد على الأقل")]
        [Display(Name = "أرقام السيريال (مفصول بينهم بفاصلة أو سطر جديد)")]
        [MaxLength(100)]
        public string DocumentNumber { get; set; }
        public int ServiceProviderId { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }
        public virtual ICollection<DocumentDetails> DocumentDetails { get; set; } = new List<DocumentDetails>();
    }
}
