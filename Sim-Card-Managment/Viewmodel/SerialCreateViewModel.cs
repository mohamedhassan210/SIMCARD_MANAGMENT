using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class SerialCreateViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "يجب إدخال رقم السيريال")]
        [MaxLength(50, ErrorMessage = "رقم السيريال لا يمكن أن يتجاوز 50 حرف")]
        [Display(Name = "رقم السيريال")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "يجب اختيار المستند المرتبط")]
        [Display(Name = "المستند المرتبط")]
        public Guid DocumentId { get; set; }

        [Display(Name = "ربط بشريحة SIM (اختياري)")]
        public Guid? SimId { get; set; }

        [Display(Name = "ربط بمودم USB (اختياري)")]
        public Guid? UsbId { get; set; }

        // معرف المستخدم الذي قام بالإجراء
        public Guid UserId { get; set; }

        // القوائم المنسدلة للواجهة (Dropdown Lists)
        public SelectList? Documents { get; set; }
        public SelectList? Sims { get; set; }
        public SelectList? Usbs { get; set; }
    }
}
