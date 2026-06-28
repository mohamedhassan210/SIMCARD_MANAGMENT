using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.Account
{
    public class AccountRepo:IAccountRepo
    {
        public readonly AppDbContext _context;
        public AccountRepo(AppDbContext context)
        {
            _context = context;
        }
        public bool Register(RegisterViewModel model)
        {
            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.PasswordHash,
                Email= model.Email,
                GroupId = model.GroupId,
                IsActive = true,
                CreatedAt= DateTime.Now,
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }
         public bool Login(LoginViewmodel model)
        {
            var checkUser = _context.Users.Any(u=>u.Username==model.Username&&u.PasswordHash==model.Password);
            return true;
        }
    }
}
