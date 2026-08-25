using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class DeviceTransferController : Controller
    {
        private readonly IDeviceTransferRepo _repo;
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;
        private readonly IDeviceStatusRepo _deviceStatusRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly INonEmployeeRepo _nonEmployeeRepo;
        private readonly AppDbContext _context;

        public DeviceTransferController(
            IDeviceTransferRepo repo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            IDeviceStatusRepo deviceStatusRepo,
            IEmployeeRepo employeeRepo,
            INonEmployeeRepo nonEmployeeRepo,
            AppDbContext context)
        {
            _repo = repo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _deviceStatusRepo = deviceStatusRepo;
            _employeeRepo = employeeRepo;
            _nonEmployeeRepo = nonEmployeeRepo;
            _context = context;
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

        // GET: DeviceTransfer/Create?simId=5 OR ?usbId=3
        [HttpGet]
        public IActionResult Create(int? simId, int? usbId)
        {
            Subscription? activeSubscription = null;

            if (simId.HasValue)
            {
                activeSubscription = _repo.GetActiveSubscriptionBySimId(simId.Value);
            }
            else if (usbId.HasValue)
            {
                activeSubscription = _repo.GetActiveSubscriptionByUsbId(usbId.Value);
            }

            if (activeSubscription == null || (activeSubscription.EndDate.HasValue && activeSubscription.EndDate.Value <= DateTime.Now))
            {
                TempData["ErrorMessage"] = "The device is not currently assigned to an active subscription or could not be found.";
                return RedirectToAction("Index");
            }

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

                    // The device is still assigned (now to the new recipient) — keep it Occupied
                    if (newSubscription.SimId.HasValue && newSubscription.SimId != 0)
                    {
                        var transferredSim = _simRepo.GetById(newSubscription.SimId.Value);
                        if (transferredSim != null)
                        {
                            transferredSim.Status = "Occupied";
                            _simRepo.Update(transferredSim);
                        }
                    }

                    if (newSubscription.UsbId.HasValue)
                    {
                        var transferredUsb = _usbRepo.GetById(newSubscription.UsbId.Value);
                        if (transferredUsb != null)
                        {
                            transferredUsb.Status = "Occupied";
                            _usbRepo.Update(transferredUsb);
                        }
                    }

                    // EF Core will save both and populate the FK automatically in 1 transaction
                    _repo.CompleteTransaction();

                    // Log this transfer in DeviceStatus history, same as SubscriptionController does — status "Occupied"
                    var occupiedStatusType = _context.DeviceStatusesType
                        .FirstOrDefault(t => t.Name == "Occupied");

                    if (occupiedStatusType != null)
                    {
                        // Snapshot the recipient's name — this transfer always results in an
                        // "Occupied" state, so the log should always carry the new owner's name.
                        string? assignedToName = _employeeRepo.GetById(deviceTransfer.ToEmpId!.Value)?.Name;

                        var deviceStatus = new DeviceStatus
                        {
                            SimId = newSubscription.SimId,
                            UsbId = newSubscription.UsbId,
                            StatusTypeId = occupiedStatusType.Id,
                            StatusDate = DateTime.Now,
                            Notes = "Device transferred to new owner",
                            ReportedBy = deviceTransfer.CreatedBy,
                            AssignedToName = assignedToName
                        };

                        // AddDeviceStatus already calls SaveChanges() internally
                        _deviceStatusRepo.AddDeviceStatus(deviceStatus);
                    }

                    return RedirectToAction("Index", "DeviceStatus");
                }

                ModelState.AddModelError("", "Active subscription to transfer from was not found.");
            }

            // If state is invalid, re-populate navigation property
            deviceTransfer.FromSubscription = _repo.GetSubscriptionById(deviceTransfer.FromSubscriptionId)!;
            return View(deviceTransfer);
        }
        [HttpGet]
        public async Task<IActionResult> SearchRecipients(string query, bool isNonEmployee, int? excludeEmpId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            if (isNonEmployee)
            {
                var nonEmployees = await _nonEmployeeRepo.SearchNonEmployeesAsync(query);

                var result = nonEmployees.Select(n => new {
                    id = n.Id,
                    name = n.Name,
                    details = $"Non-Employee | Contact: {n.ContactInfo ?? "N/A"}"
                });

                return Json(result);
            }
            else
            {
                var employees = await _employeeRepo.SearchActiveEmployeesAsync(query);

                if (excludeEmpId.HasValue)
                {
                    employees = employees.Where(e => e.Id != excludeEmpId.Value);
                }

                var result = employees.Select(e => new {
                    id = e.Id,
                    name = e.Name,
                    details = $"National ID: {e.NationalID}"
                });

                return Json(result);
            }
        }
    }
}