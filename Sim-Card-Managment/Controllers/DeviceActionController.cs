using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;

namespace Sim_Card_Managment.Controllers
{
    public class DeviceActionController : Controller
    {
        private readonly IDeviceActionRepo _repo;

        public DeviceActionController(IDeviceActionRepo repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var actions = _repo.GetAllDeviceActions();
            return View(actions);
        }

        public IActionResult Create()
        {
            return View(new DeviceAction());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeviceAction deviceAction)
        {
            if (ModelState.IsValid)
            {
                deviceAction.Id = Guid.NewGuid();
                _repo.AddDeviceAction(deviceAction);
                return RedirectToAction(nameof(Index));
            }
            return View(deviceAction);
        }
    }
}