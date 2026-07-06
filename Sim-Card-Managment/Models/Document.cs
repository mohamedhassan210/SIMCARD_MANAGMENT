using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumenttypeId { get; set; }
        public DocumentType DocumentType { get; set; }
        [Required(ErrorMessage = "برجاء اختيار نوع المستند")]
        public DateTime ActionDate { get; set; }
        public string Notes { get; set; }
        public string SignatureType { get; set; }

        public string SignatureData { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }

        public User CreatedBy { get; set; }

        [Required(ErrorMessage = "يجب إدخال سيريال واحد على الأقل")]
        [Display(Name = "أرقام السيريال (مفصول بينهم بفاصلة أو سطر جديد)")]
        public string DocumentNumber { get; set; }

        public virtual ICollection<Serial> Serials { get; set; } = new List<Serial>();
    }
}
