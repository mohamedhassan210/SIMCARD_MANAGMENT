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
        private readonly AppDbContext _context;
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
            int currentUserId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 1;

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

            // Carry each device's current Status so the view can show it read-only
            model.Sims = sims
                .Select(s => new DeviceOptionViewModel { Id = s.Id, SerialNumber = s.SerialNumber, Status = s.Status })
                .ToList();

            model.Usbs = usbs
                .Select(u => new DeviceOptionViewModel { Id = u.Id, SerialNumber = u.SerialNumber, Status = u.Status })
                .ToList();

            model.ReplacementSims = new SelectList(sims, "Id", "SerialNumber", model.ReplacedBySimId);
            model.ReplacementUsbs = new SelectList(usbs, "Id", "SerialNumber", model.ReplacedByUsbId);

            // Always pulled fresh from the DeviceStatusesType table
            model.StatusTypes = new SelectList(
                _context.DeviceStatusesType.OrderBy(t => t.Name).ToList(),
                "Id", "Name", model.StatusTypeId);
        }
        [HttpGet]
        public async Task<IActionResult> SearchSims(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var sims = await _simRepo.SearchAsync(query);

            var result = sims.Select(s => new
            {
                id = s.Id,
                phoneNumber = s.PhoneNumber,
                serialNumber = s.SerialNumber,
                status = s.Status,
                providerName = s.ServiceProvider?.Name ?? "Unknown"
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsbs(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var usbs = await _usbRepo.GetAvailableUsbsAsync(query);

            var result = usbs.Select(u => new
            {
                id = u.Id,
                model = u.Model,
                serialNumber = u.SerialNumber,
                status = u.Status,
                providerName = u.ServiceProvider?.Name ?? "Unknown"
            });

            return Json(result);
        }
    }
}