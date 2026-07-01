using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the OTP code sent to your email.")]
        [StringLength(10, ErrorMessage = "Invalid OTP format.")]
        public string OtpCode { get; set; } = string.Empty;
    }
}