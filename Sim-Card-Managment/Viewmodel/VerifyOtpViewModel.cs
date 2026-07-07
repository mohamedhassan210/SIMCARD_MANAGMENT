using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{

    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OtpCode { get; set; } = string.Empty;
    }
}