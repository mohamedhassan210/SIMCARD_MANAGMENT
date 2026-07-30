// DocumentCreateViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sim_Card_Managment.Viewmodel
{
    public class DocumentCreateViewModel
    {
        [Display(Name = "نوع المستند")]
        public int DocumentTypeId { get; set; }

        [Display(Name = "مزود الخدمة")]
        public int? ServiceProviderId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ActionDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [MaxLength(50)]
        [Display(Name = "نوع التوقيع")]
        public string? SignatureType { get; set; }

        [MaxLength(50)]
        [Display(Name = "بيانات التوقيع")]
        public string? SignatureData { get; set; }

        [Display(Name = "رقم المستند")]
        public string? DocumentNumber { get; set; }

        public List<SimItemViewModel> Sims { get; set; } = new();
        public List<UsbItemViewModel> Usbs { get; set; } = new();

        public List<SelectListItem> DocumentTypes { get; set; } = new();
        public List<SelectListItem> ServiceProviders { get; set; } = new();
    }

    public class SimItemViewModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "رقم السيريال (ICCID)")]
        public string SerialNumber { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [MaxLength(10)]
        [Display(Name = "نوع الشبكة")]
        public string? NetworkType { get; set; }
    }

    public class UsbItemViewModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "رقم السيريال")]
        public string SerialNumber { get; set; }

        [MaxLength(200)]
        [Display(Name = "الموديل")]
        public string? Model { get; set; }
    }
}