using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;

        public DeviceStatusController(AppDbContext context, ISIMRepo simRepo, IUSBRepo usbRepo)
        {
            _context = context;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
        }

        // GET: /DeviceStatus
        public IActionResult Index()
        {
            var statuses = _context.DeviceStatuses
                .Include(ds => ds.Sim)
                    .ThenInclude(s => s!.Subscriptions)
                        .ThenInclude(sub => sub.Employee)
                .Include(ds => ds.Sim)
                    .ThenInclude(s => s!.Subscriptions)
                        .ThenInclude(sub => sub.NonEmployee)
                .Include(ds => ds.Usb)
                    .ThenInclude(u => u!.Subscriptions)
                        .ThenInclude(sub => sub.Employee)
                .Include(ds => ds.Usb)
                    .ThenInclude(u => u!.Subscriptions)
                        .ThenInclude(sub => sub.NonEmployee)
                .Include(ds => ds.StatusType)
                .Include(ds => ds.ReportedByUser)
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
            if (!model.SimId.HasValue && !model.UsbId.HasValue)
            {
                ModelState.AddModelError("", "Please select a SIM or a USB device to report on.");
            }
            else if (model.SimId.HasValue && model.UsbId.HasValue)
            {
                ModelState.AddModelError("", "Please select only one device — a SIM or a USB, not both.");
            }

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

            _context.DeviceStatuses.Add(deviceStatus);

            // Reflect this incident on the device's own live Status field too
            if (statusType != null)
            {
                if (model.SimId.HasValue)
                {
                    var sim = _simRepo.GetById(model.SimId.Value);
                    if (sim != null)
                    {
                        sim.Status = statusType.Name;
                        _simRepo.Update(sim);
                    }
                }
                else if (model.UsbId.HasValue)
                {
                    var usb = _usbRepo.GetById(model.UsbId.Value);
                    if (usb != null)
                    {
                        usb.Status = statusType.Name;
                        _usbRepo.Update(usb);
                    }
                }
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit()
        {

            return View();
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