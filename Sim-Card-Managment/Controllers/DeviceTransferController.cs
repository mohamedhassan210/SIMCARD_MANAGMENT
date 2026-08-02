using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
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

        public IActionResult Details(int id)
        {
            var deviceTransfer = _repo.GetDeviceTransferbyId(id);
            if (deviceTransfer == null) return NotFound();

            return View(deviceTransfer);
        }

        // GET: DeviceTransfer/Create?simId=5 OR ?usbId=3 OR ?subscriptionId=10
        [HttpGet]
        public IActionResult Create(int? simId)
        {
            Subscription? activeSubscription = null;

            // 1. Retrieve the active subscription based on passed ID
            
            if (simId.HasValue)
            {
                activeSubscription = _repo.GetActiveSubscriptionBySimId(simId.Value);
            }


            // ✅ CORRECT: Only rejects if null OR if EndDate is in the past
            if (activeSubscription == null || (activeSubscription.EndDate.HasValue && activeSubscription.EndDate.Value <= DateTime.Now))
            {
                TempData["ErrorMessage"] = "The device is not currently assigned to an active subscription or could not be found.";
                return RedirectToAction("Index");
            }

            // 3. Pre-populate the transfer model
            var transferModel = new DeviceTransfer
            {
                FromSubscriptionId = activeSubscription.Id,
                SimId = activeSubscription.SimId,
                UsbId = activeSubscription.UsbId,
                TransferDate = DateTime.Now,
                FromSubscription = activeSubscription
            };

            return View(transferModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeviceTransfer deviceTransfer)
        {
            // 1. Get logged-in user ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                deviceTransfer.CreatedBy = currentUserId;
            }
            else
            {
                deviceTransfer.CreatedBy = 1; // Fallback default user
            }

            // 2. Clear validation errors for navigation properties
            ModelState.Remove(nameof(DeviceTransfer.FromSubscription));
            ModelState.Remove(nameof(DeviceTransfer.NewSubscription)); // Added
            ModelState.Remove(nameof(DeviceTransfer.ToEmployee));
            ModelState.Remove(nameof(DeviceTransfer.CreatedByUser));
            ModelState.Remove(nameof(DeviceTransfer.Sim));
            ModelState.Remove(nameof(DeviceTransfer.Usb));

            // 3. Validate target employee selection explicitly
            if (deviceTransfer.ToEmpId == null || deviceTransfer.ToEmpId == 0)
            {
                ModelState.AddModelError("ToEmpId", "Please search and select a target recipient employee.");
            }

            if (ModelState.IsValid)
            {
                var oldSubscription = _repo.GetSubscriptionById(deviceTransfer.FromSubscriptionId);

                if (oldSubscription != null)
                {
                    // Close old subscription on transfer date
                    oldSubscription.EndDate = deviceTransfer.TransferDate;

                    // Create new active subscription for recipient
                    var newSubscription = new Subscription
                    {
                        EmpId = deviceTransfer.ToEmpId,
                        SimId = deviceTransfer.SimId ?? oldSubscription.SimId,
                        UsbId = deviceTransfer.UsbId ?? oldSubscription.UsbId,
                        CreatedBy = deviceTransfer.CreatedBy,
                        CreatedDate = DateTime.Now,
                        StartDate = deviceTransfer.TransferDate,
                        EndDate = null, // Active
                        QuotaId = oldSubscription.QuotaId,
                        ActionId = oldSubscription.ActionId
                    };

                    _repo.AddSubscription(newSubscription);

                    // ✅ Link using Navigation Property instead of integer ID
                    deviceTransfer.NewSubscription = newSubscription;

                    _repo.AddDeviceTransfer(deviceTransfer);

                    // EF Core will save both and populate the FK automatically in 1 transaction
                    _repo.CompleteTransaction();

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Active subscription to transfer from was not found.");
            }

            // If state is invalid, re-populate navigation property
            deviceTransfer.FromSubscription = _repo.GetSubscriptionById(deviceTransfer.FromSubscriptionId)!;
            return View(deviceTransfer);
        }
    }
}