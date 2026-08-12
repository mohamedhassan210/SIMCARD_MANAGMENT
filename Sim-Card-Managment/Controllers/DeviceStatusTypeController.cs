using Microsoft.AspNetCore.Mvc;
using Sim_Card_Management.Models;
using Sim_Card_Managment.Repos.DeviceStatusTypeRepo;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class DeviceStatusTypeController : Controller
    {
        private readonly IDeviceStatusTypeRepo _repo;

        public DeviceStatusTypeController(IDeviceStatusTypeRepo repo)
        {
            _repo = repo;
        }

        // GET: DeviceStatusType
        public IActionResult Index()
        {
            var types = _repo.GetAll();
            return View(types);
        }

        // GET: DeviceStatusType/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeviceStatusType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeviceStatusType deviceStatusType)
        {
            if (ModelState.IsValid)
            {
                _repo.Add(deviceStatusType);
                TempData["Success"] = "Device status type added successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(deviceStatusType);
        }

        // GET: DeviceStatusType/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _repo.GetById(id);
            if (entity == null)
                return NotFound();

            return View(entity);
        }

        // POST: DeviceStatusType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DeviceStatusType deviceStatusType)
        {
            if (ModelState.IsValid)
            {
                _repo.Update(deviceStatusType);
                TempData["Success"] = "Device status type updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(deviceStatusType);
        }

        // POST: DeviceStatusType/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);
            TempData["Success"] = "Device status type deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}