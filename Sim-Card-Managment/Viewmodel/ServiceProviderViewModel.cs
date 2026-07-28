using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.ViewModels
{
    public class ServiceProviderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "برجاء إدخال اسم الشركة الفريد")]
        [Display(Name = "اسم الشركة (إنجليزي)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "برجاء إدخال الاسم المعروض للشركة")]
        [Display(Name = "الاسم المعروض بالواجهة")]
        public string DisplayName { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;
    }
}