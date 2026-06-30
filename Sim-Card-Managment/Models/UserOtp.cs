using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sim_Card_Managment.Models
{
    public class UserOtp
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string OtpCode { get; set; } = string.Empty; // Holds the generated OTP (e.g., "482910")

        [Required]
        public DateTime ExpireDate { get; set; } // Set to (DateTime.Now.AddMinutes(15)) when created

        [Required]
        public bool IsUsed { get; set; } = false; // Changes to true once they successfully log in
    }
}