using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Controllers
{
    public class DeviceTransferController : Controller
    {
        private readonly IDeviceTransferRepo _repo;

        public DeviceTransferController(IDeviceTransferRepo repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var transfers = _repo.GetAllDeviceTransfers();
            return View(transfers);
        }

        public IActionResult Details(Guid id)
        {
            var deviceTransfer = _repo.GetDeviceTransferbyId(id);
            if (deviceTransfer == null) return NotFound();

            return View(deviceTransfer);
        }

        public IActionResult Create()
        {
            return View(new DeviceTransfer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeviceTransfer deviceTransfer)
        {
            if (ModelState.IsValid)
            {
                deviceTransfer.Id = Guid.NewGuid();
                _repo.AddDeviceTransfer(deviceTransfer);
                return RedirectToAction(nameof(Index));
            }
            return View(deviceTransfer);
        }

        public IActionResult Edit(Guid id)
        {
            var deviceTransfer = _repo.GetDeviceTransferbyId(id);
            if (deviceTransfer == null) return NotFound();

            return View(deviceTransfer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, DeviceTransfer deviceTransfer)
        {
            if (id != deviceTransfer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _repo.Update(deviceTransfer);
                return RedirectToAction(nameof(Index));
            }
            return View(deviceTransfer);
        }

        public IActionResult Delete(Guid id)
        {
            var deviceTransfer = _repo.GetDeviceTransferbyId(id);
            if (deviceTransfer == null) return NotFound();

            return View(deviceTransfer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _repo.DeleteTransfer(id);
            return RedirectToAction(nameof(Index));
        }
    }
}