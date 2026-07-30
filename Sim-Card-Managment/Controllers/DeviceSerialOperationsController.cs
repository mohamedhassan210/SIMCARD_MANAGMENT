using Microsoft.AspNetCore.Mvc;

namespace Sim_Card_Management.Controllers
{
    public class DeviceSerialOperationsController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
    }
}
