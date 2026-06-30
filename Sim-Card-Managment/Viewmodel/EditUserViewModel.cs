using System;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class EditUserViewModel
    {
        [Required]
        public Guid Id { get; set; }

        // الـ Username للعرض فقط في الشاشة وليس للتعديل
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "يجب تحديد المجموعة")]
        public Guid GroupId { get; set; }

        [Required(ErrorMessage = "يجب تحديد الصلاحية")]
        public string Role { get; set; } = string.Empty; // "Manager" أو "Employee"
    }
}