using Microsoft.AspNetCore.Mvc;

namespace Sim_Card_Management.Controllers
{
    [RequirePermission]
    public class PermissionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
