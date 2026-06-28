using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;

namespace Sim_Card_Managment.Controllers
{
    public class DeviceStatusController : Controller
    {
        private readonly IDeviceStatusRepo _repo;

        public DeviceStatusController(IDeviceStatusRepo repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var statuses = _repo.GetAllDeviceStatuses();
            return View(statuses);
        }

        public IActionResult Details(Guid id)
        {
            var deviceStatus = _repo.GetDeviceStatusbyId(id);
            if (deviceStatus == null) return NotFound();

            return View(deviceStatus);
        }

        public IActionResult Create()
        {
            return View(new DeviceStatus { StatusDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeviceStatus deviceStatus)
        {
            if (ModelState.IsValid)
            {
                deviceStatus.Id = Guid.NewGuid();
                _repo.AddDeviceStatus(deviceStatus);
                return RedirectToAction(nameof(Index));
            }
            return View(deviceStatus);
        }

        public IActionResult Edit(Guid id)
        {
            var deviceStatus = _repo.GetDeviceStatusbyId(id);
            if (deviceStatus == null) return NotFound();

            return View(deviceStatus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, DeviceStatus deviceStatus)
        {
            if (id != deviceStatus.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _repo.Update(deviceStatus);
                return RedirectToAction(nameof(Index));
            }
            return View(deviceStatus);
        }

        public IActionResult Delete(Guid id)
        {
            var deviceStatus = _repo.GetDeviceStatusbyId(id);
            if (deviceStatus == null) return NotFound();

            return View(deviceStatus);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _repo.DeleteStatus(id);
            return RedirectToAction(nameof(Index));
        }
    }
}