using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class DocumentCreateViewModel
    {
        [Display(Name = "نوع المستند")]
        [Required(ErrorMessage = "يجب اختيار نوع المستند")]
        public int? DocumentTypeId { get; set; }

        [Display(Name = "مزود الخدمة")]
        [Required(ErrorMessage = "يجب اختيار مزود الخدمة")]
        public int? ServiceProviderId { get; set; }

        [Display(Name = "تاريخ الإجراء")]
        [Required(ErrorMessage = "يجب إدخال تاريخ الإجراء")]
        public DateTime ActionDate { get; set; } = DateTime.Now;

        [Display(Name = "ملاحظات")]
        [MaxLength(500)]
        public string Notes { get; set; }

        [Display(Name = "نوع التوقيع")]
        [MaxLength(50)]
        public string SignatureType { get; set; }

        [Display(Name = "بيانات التوقيع")]
        [MaxLength(50)]
        public string SignatureData { get; set; }

        [Display(Name = "رقم المستند")]
        [Required(ErrorMessage = "يجب إدخال سيريال واحد على الأقل")]
        [MaxLength(100)]
        public string DocumentNumber { get; set; }

        // Collections for dynamic rows
        public List<SimCreateDto> Sims { get; set; } = new List<SimCreateDto>();
        public List<UsbCreateDto> Usbs { get; set; } = new List<UsbCreateDto>();

        // For dropdown population
        public List<SelectListItem> DocumentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceProviders { get; set; } = new List<SelectListItem>();
    }

    public class SimCreateDto
    {
        [Required(ErrorMessage = "السيريال مطلوب")]
        [StringLength(100)]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [StringLength(10)]
        public string NetworkType { get; set; }
    }

    public class UsbCreateDto
    {
        [Required(ErrorMessage = "السيريال مطلوب")]
        [StringLength(100)]
        public string SerialNumber { get; set; }

        [StringLength(200)]
        public string Model { get; set; }
    }
}