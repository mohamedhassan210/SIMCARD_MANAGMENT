using Microsoft.AspNetCore.Mvc;

namespace Sim_Card_Management.Controllers
{
    public class PermissionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MangePermission() { return View(); } 
    }
}
