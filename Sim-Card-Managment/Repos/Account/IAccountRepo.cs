using System.Threading.Tasks;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.Account
{
    public interface IAccountRepo
    {
        bool Register(RegisterViewModel model);
        Task<bool> Login(LoginViewmodel model); 
        Task Logout(); 
    }
}