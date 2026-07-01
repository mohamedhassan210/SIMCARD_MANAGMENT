using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class LoginViewmodel
    {
        [Required(ErrorMessage = "The UserName Is Required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Password Is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}