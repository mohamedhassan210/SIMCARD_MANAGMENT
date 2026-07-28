using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class DocumentCreateViewModel
    {
        [Required(ErrorMessage = "برجاء اختيار نوع المستند")]
        [Display(Name = "نوع المستند")]
        public int DocumentTypeId { get; set; }

        [Required(ErrorMessage = "برجاء اختيار تاريخ الإجراء")]
        [Display(Name = "تاريخ الإجراء")]
        [DataType(DataType.Date)]
        public DateTime ActionDate { get; set; } = DateTime.Today;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "نوع التوقيع")]
        public string? SignatureType { get; set; }

        [Display(Name = "بيانات التوقيع")]
        public string? SignatureData { get; set; }

        [Required(ErrorMessage = "يجب إدخال سيريال واحد على الأقل")]
        [Display(Name = "أرقام السيريال (مفصول بينهم بفاصلة أو سطر جديد)")]
        public string DocumentNumber { get; set; }

        // لربط السيريالات بـ SIM أو USB اختيارياً عند الإنشاء
        [Display(Name = "ربط بـ SIM Card (اختياري)")]
        public int? SelectedSimId { get; set; }

        [Display(Name = "ربط بـ USB Modem (اختياري)")]
        public int? SelectedUsbId { get; set; }

        // القوائم المنسدلة للواجهة (الداتا لييست)
        public SelectList? DocumentTypes { get; set; }
        public SelectList? Sims { get; set; }
        public SelectList? Usbs { get; set; }
    }
}
