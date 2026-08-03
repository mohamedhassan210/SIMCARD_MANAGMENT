using System;
namespace Sim_Card_Managment.Viewmodel
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;  
        public bool IsActive { get; set; }                     
    }
}