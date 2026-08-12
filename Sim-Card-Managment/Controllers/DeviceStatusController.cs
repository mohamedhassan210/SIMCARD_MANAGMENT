using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class DeviceStatusController : Controller
    {
        private readonly AppDbContext _context; // still needed for the DeviceStatusesType lookup table
        private readonly IDeviceStatusRepo _deviceStatusRepo;
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;

        public DeviceStatusController(AppDbContext context, IDeviceStatusRepo deviceStatusRepo, ISIMRepo simRepo, IUSBRepo usbRepo)
        {
            _context = context;
            _deviceStatusRepo = deviceStatusRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
        }

        // GET: /DeviceStatus
        public IActionResult Index()
        {
            var statuses = _deviceStatusRepo.GetAllDeviceStatuses()
                .OrderBy(ds => ds.StatusDate)
                .ToList();

            var viewModel = new List<DeviceStatusViewModel>();

            // Tracks the most recent status seen per device so we can show old -> new per row
            var lastStatus = new Dictionary<string, string>();

            foreach (var ds in statuses)
            {
                string deviceKey = ds.SimId.HasValue ? $"sim-{ds.SimId}" : $"usb-{ds.UsbId}";
                string oldStatus = lastStatus.TryGetValue(deviceKey, out var prev) ? prev : "Unassigned";
                string newStatus = ds.StatusType?.Name ?? "Unknown";
                lastStatus[deviceKey] = newStatus;

                string serialNumber = ds.Sim?.SerialNumber ?? ds.Usb?.SerialNumber ?? "N/A";
                string deviceType = ds.SimId.HasValue ? "SIM Card" : "USB Modem";
                bool isActive = ds.Sim?.IsActive ?? ds.Usb?.IsActive ?? false;

                string? assignedTo = ds.Sim != null
                    ? ds.Sim.Subscriptions?
                        .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                        .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                        .FirstOrDefault()
                    : ds.Usb?.Subscriptions?
                        .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                        .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                        .FirstOrDefault();

                viewModel.Add(new DeviceStatusViewModel
                {
                    Id = ds.Id,
                    SerialNumber = serialNumber,
                    DeviceType = deviceType,
                    Notes = ds.Notes,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    IsActive = isActive,
                    AssignedTo = assignedTo ?? "Unassigned",
                    ReportedByUserName = ds.ReportedByUser?.Username ?? "N/A",
                    StatusDate = ds.StatusDate
                });
            }

            // Most recent report first
            viewModel.Reverse();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new DeviceStatusCreateViewModel();
            PopulateLookupLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeviceStatusCreateViewModel model)
        {
            ValidateDeviceSelection(model.SimId, model.UsbId);

            if (!ModelState.IsValid)
            {
                PopulateLookupLists(model);
                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 1; // fallback default user

            var statusType = _context.DeviceStatusesType.FirstOrDefault(t => t.Id == model.StatusTypeId);

            var deviceStatus = new DeviceStatus
            {
                SimId = model.SimId,
                UsbId = model.UsbId,
                StatusTypeId = model.StatusTypeId,
                StatusDate = DateTime.Now,
                Notes = model.Notes,
                ReportedBy = currentUserId,
                ReplacedBySimId = model.ReplacedBySimId,
                ReplacedByUsbId = model.ReplacedByUsbId
            };

            _deviceStatusRepo.AddDeviceStatus(deviceStatus);

            // Reflect this incident on the device's own live Status field too
            ApplyStatusToDevice(model.SimId, model.UsbId, statusType);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var status = _deviceStatusRepo.GetDeviceStatusbyId(id);
            if (status == null) return NotFound();

            var model = new DeviceStatusEditViewModel
            {
                Id = status.Id,
                SimId = status.SimId,
                UsbId = status.UsbId,
                StatusTypeId = status.StatusTypeId,
                Notes = status.Notes,
                ReplacedBySimId = status.ReplacedBySimId,
                ReplacedByUsbId = status.ReplacedByUsbId
            };

            PopulateLookupLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DeviceStatusEditViewModel model)
        {
            ValidateDeviceSelection(model.SimId, model.UsbId);

            if (!ModelState.IsValid)
            {
                PopulateLookupLists(model);
                return View(model);
            }

            var status = _deviceStatusRepo.GetDeviceStatusbyId(model.Id);
            if (status == null) return NotFound();

            var statusType = _context.DeviceStatusesType.FirstOrDefault(t => t.Id == model.StatusTypeId);

            status.SimId = model.SimId;
            status.UsbId = model.UsbId;
            status.StatusTypeId = model.StatusTypeId;
            status.Notes = model.Notes;
            status.ReplacedBySimId = model.ReplacedBySimId;
            status.ReplacedByUsbId = model.ReplacedByUsbId;

            _deviceStatusRepo.Update(status);

            // Keep the device's live Status field in sync with the (possibly changed) status type
            ApplyStatusToDevice(model.SimId, model.UsbId, statusType);

            return RedirectToAction(nameof(Index));
        }

        private void ValidateDeviceSelection(int? simId, int? usbId)
        {
            if (!simId.HasValue && !usbId.HasValue)
            {
                ModelState.AddModelError("", "Please select a SIM or a USB device to report on.");
            }
            else if (simId.HasValue && usbId.HasValue)
            {
                ModelState.AddModelError("", "Please select only one device — a SIM or a USB, not both.");
            }
        }

        private void ApplyStatusToDevice(int? simId, int? usbId, DeviceStatusType? statusType)
        {
            if (statusType == null) return;

            if (simId.HasValue)
            {
                var sim = _simRepo.GetById(simId.Value);
                if (sim != null)
                {
                    sim.Status = statusType.Name;
                    _simRepo.Update(sim);
                }
            }
            else if (usbId.HasValue)
            {
                var usb = _usbRepo.GetById(usbId.Value);
                if (usb != null)
                {
                    usb.Status = statusType.Name;
                    _usbRepo.Update(usb);
                }
            }
        }

        private void PopulateLookupLists(DeviceStatusCreateViewModel model)
        {
            var sims = _simRepo.GetAll().ToList();
            var usbs = _usbRepo.GetAll().ToList();

            model.Sims = new SelectList(sims, "Id", "SerialNumber", model.SimId);
            model.Usbs = new SelectList(usbs, "Id", "SerialNumber", model.UsbId);
            model.ReplacementSims = new SelectList(sims, "Id", "SerialNumber", model.ReplacedBySimId);
            model.ReplacementUsbs = new SelectList(usbs, "Id", "SerialNumber", model.ReplacedByUsbId);
            model.StatusTypes = new SelectList(
                _context.DeviceStatusesType.OrderBy(t => t.Name).ToList(),
                "Id", "Name", model.StatusTypeId);
        }
    }
}