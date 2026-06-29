using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;

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
            return View(new DeviceTransfer { TransferDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeviceTransfer deviceTransfer)
        {
            if (ModelState.IsValid)
            {
                var oldSubscription = _repo.GetSubscriptionById(deviceTransfer.FromSubscriptionId);

                if (oldSubscription != null)
                {
                    // 1. Close the old subscription by setting EndDate to Now
                    oldSubscription.EndDate = deviceTransfer.TransferDate;

                    // 2. Create the new subscription (leave EndDate as null so it is Active)
                    var newSubscription = new Subscription
                    {
                        Id = Guid.NewGuid(),
                        EmpId = deviceTransfer.ToEmpId,
                        SimId = deviceTransfer.SimId ?? oldSubscription.SimId,
                        UsbId = deviceTransfer.UsbId ?? oldSubscription.UsbId,
                        CreatedBy = deviceTransfer.CreatedBy,
                        CreatedDate = DateTime.Now,
                        StartDate = deviceTransfer.TransferDate,
                        EndDate = null, // Open-ended/Active
                        QuotaId = oldSubscription.QuotaId,
                        ActionId = oldSubscription.ActionId
                    };

                    deviceTransfer.Id = Guid.NewGuid();
                    deviceTransfer.NewSubscriptionId = newSubscription.Id;

                    _repo.AddSubscription(newSubscription);
                    _repo.AddDeviceTransfer(deviceTransfer);
                    _repo.CompleteTransaction();

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Old subscription not found.");
            }
            return View(deviceTransfer);
        }
    }
}