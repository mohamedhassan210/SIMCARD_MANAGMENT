using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Viewmodel;
namespace Sim_Card_Managment.Controllers
{
    public class Accountcontroller : Controller
    {
        private readonly IAccountRepo _accountRepo;
        public Accountcontroller(IAccountRepo accountRepo)
        {
            _accountRepo=accountRepo;   
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var newuser=_accountRepo.Register(model);   
            if (newuser)
            {
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("","حدث خطأ اثناء التسجيل حاول مره اخرى");
            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewmodel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var Isfound=_accountRepo.Login(model);
            if(Isfound)
            {
                return RedirectToAction("Index","Home");
            }
            ModelState.AddModelError("","اسم المستخدم او كلمه المرور غير صحيحه");
            return View(model);
        }
    }
}
