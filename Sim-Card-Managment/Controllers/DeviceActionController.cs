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

        
        public IActionResult Details(Guid id)
        {
            var deviceAction = _repo.GetDeviceActionbyId(id);
            if (deviceAction == null) return NotFound();

            return View(deviceAction);
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

        
        public IActionResult Edit(Guid id)
        {
            var deviceAction = _repo.GetDeviceActionbyId(id);
            if (deviceAction == null) return NotFound();

            return View(deviceAction);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, DeviceAction deviceAction)
        {
            if (id != deviceAction.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _repo.Update(deviceAction);
                return RedirectToAction(nameof(Index));
            }
            return View(deviceAction);
        }

        
        public IActionResult Delete(Guid id)
        {
            var deviceAction = _repo.GetDeviceActionbyId(id);
            if (deviceAction == null) return NotFound();

            return View(deviceAction);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _repo.DeleteAction(id);
            return RedirectToAction(nameof(Index));
        }
    }
}