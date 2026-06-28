using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class LoginViewmodel
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
