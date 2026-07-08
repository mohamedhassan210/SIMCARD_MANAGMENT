using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class DocumentTypeViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "برجاء إدخال اسم النظام الفريد (Name)")]
        [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
        [Display(Name = "اسم النظام (إنجليزي/فريد)")]
        public string Name { get; set; }

        
        [Required(ErrorMessage = "برجاء إدخال الاسم المعروض (Display Name)")]
        [MaxLength(150, ErrorMessage = "الاسم المعروض لا يمكن أن يتجاوز 150 حرف")]
        [Display(Name = "الاسم المعروض بالواجهة")]
        public string DisplayName { get; set; }
    }
}
