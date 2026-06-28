using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.Account
{
    public interface IAccountRepo
    {
      public  bool Register(RegisterViewModel model);
      public  bool Login(LoginViewmodel model);

    }
}
