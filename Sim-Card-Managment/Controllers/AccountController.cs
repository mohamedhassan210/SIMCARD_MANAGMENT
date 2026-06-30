using Microsoft.AspNetCore.Mvc;

namespace Sim_Card_Managment.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
