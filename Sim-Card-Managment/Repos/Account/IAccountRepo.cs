using Sim_Card_Managment.Models;
using Sim_Card_Managment.Viewmodel;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Repos.Account
{
    public interface IAccountRepo
    {
        UserOtp? GetValidOtpByEmail(string email);
        User? GetUserByEmail(string email);
        bool Register(RegisterViewModel model);
        Task<bool> Login(LoginViewmodel model); 
        Task Logout(); 
    }
}