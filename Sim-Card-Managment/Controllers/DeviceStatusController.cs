using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using System.Drawing;
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
        private readonly ISubscriptionRepo _subscriptionRepo;

        public DeviceStatusController(
            AppDbContext context,
            IDeviceStatusRepo deviceStatusRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            ISubscriptionRepo subscriptionRepo)
        {
            _context = context;
            _deviceStatusRepo = deviceStatusRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _subscriptionRepo = subscriptionRepo;
        }

        // GET: /DeviceStatus
        public IActionResult Index()
        {
            var statuses = _deviceStatusRepo.GetAllDeviceStatuses()
                .OrderBy(ds => ds.StatusDate)
                .ToList();

            var viewModel = BuildDeviceStatusViewModels(statuses);
            viewModel.Reverse();
            return View(viewModel);
        }

        /// <summary>
        /// GET: /DeviceStatus/ExportDeviceStatusExcel
        /// Exports the device status log to Excel, filtered by the same
        /// dropdown filters the Index view offers (Availability, Device Type),
        /// plus an optional StatusDate range.
        /// Free-text search is intentionally excluded — it's client-side only.
        /// </summary>
        [HttpGet]
        public IActionResult ExportDeviceStatusExcel(bool? isActive, string? deviceType, DateTime? from, DateTime? to)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            // Same repo call + same transformation logic as Index — then filter in memory,
            // since GetAllDeviceStatuses() has no server-side filter parameters.
            var statuses = _deviceStatusRepo.GetAllDeviceStatuses()
                .OrderBy(ds => ds.StatusDate)
                .ToList();

            var records = BuildDeviceStatusViewModels(statuses);
            records.Reverse();

            if (isActive.HasValue)
            {
                records = records.Where(r => r.IsActive == isActive.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(deviceType))
            {
                records = records.Where(r => string.Equals(r.DeviceType, deviceType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (from.HasValue)
            {
                records = records.Where(r => r.StatusDate.Date >= from.Value.Date).ToList();
            }

            if (to.HasValue)
            {
                // Inclusive of the whole "to" day, same as a person would expect
                // from a date picker labeled "To: 2026-08-26".
                records = records.Where(r => r.StatusDate.Date <= to.Value.Date).ToList();
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Device Status Log");

                worksheet.Cells[1, 1].Value = "Serial Number";
                worksheet.Cells[1, 2].Value = "Device Type";
                worksheet.Cells[1, 3].Value = "Phone Number / USB Model";
                worksheet.Cells[1, 4].Value = "Old Status";
                worksheet.Cells[1, 5].Value = "New Status";
                worksheet.Cells[1, 6].Value = "Availability";
                worksheet.Cells[1, 7].Value = "Assigned To";
                worksheet.Cells[1, 8].Value = "Reported By";
                worksheet.Cells[1, 9].Value = "Status Date";
                worksheet.Cells[1, 10].Value = "Notes";

                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightSlateGray);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                int row = 2;
                foreach (var r in records)
                {
                    worksheet.Cells[row, 1].Value = r.SerialNumber;
                    worksheet.Cells[row, 2].Value = r.DeviceType;
                    worksheet.Cells[row, 3].Value = r.Identifier ?? "N/A";
                    worksheet.Cells[row, 4].Value = r.OldStatus;
                    worksheet.Cells[row, 5].Value = r.NewStatus;
                    worksheet.Cells[row, 6].Value = r.Availability;
                    worksheet.Cells[row, 7].Value = r.AssignedTo;
                    worksheet.Cells[row, 8].Value = r.ReportedByUserName;
                    worksheet.Cells[row, 9].Value = r.StatusDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[row, 10].Value = r.Notes ?? "";
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileNameParts = new List<string>();
                if (isActive.HasValue) fileNameParts.Add(isActive.Value ? "Active" : "NotActive");
                if (!string.IsNullOrWhiteSpace(deviceType)) fileNameParts.Add(deviceType.Replace(" ", ""));
                if (from.HasValue || to.HasValue)
                {
                    var fromPart = from.HasValue ? from.Value.ToString("yyyyMMdd") : "Start";
                    var toPart = to.HasValue ? to.Value.ToString("yyyyMMdd") : "Now";
                    fileNameParts.Add($"{fromPart}-{toPart}");
                }

                var suffix = fileNameParts.Any() ? "_" + string.Join("_", fileNameParts) : "";
                var fileName = $"DeviceStatus{suffix}_{DateTime.Now:yyyyMMdd}.xlsx";

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        private List<DeviceStatusViewModel> BuildDeviceStatusViewModels(List<DeviceStatus> statuses)
        {
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
                string? identifier = ds.Sim?.PhoneNumber ?? ds.Usb?.Model;
                bool isActive = ds.Sim?.IsActive ?? ds.Usb?.IsActive ?? false;

                viewModel.Add(new DeviceStatusViewModel
                {
                    Id = ds.Id,
                    SerialNumber = serialNumber,
                    DeviceType = deviceType,
                    Identifier = identifier,
                    Notes = ds.Notes,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    IsActive = isActive,
                    // Read the snapshot taken at write time instead of computing it live —
                    // that's exactly what made past rows lose their assignee once the
                    // subscription behind them was closed.
                    AssignedTo = ds.AssignedToName ?? "Unassigned",
                    ReportedByUserName = ds.ReportedByUser?.Username ?? "N/A",
                    StatusDate = ds.StatusDate
                });
            }

            return viewModel;
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

            if (!model.StatusTypeId.HasValue || model.StatusTypeId.Value == 0)
            {
                ModelState.AddModelError(nameof(model.StatusTypeId), "Please select a status.");
            }

            if (!ModelState.IsValid)
            {
                PopulateLookupLists(model);
                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 1;

            var statusType = _context.DeviceStatusesType.FirstOrDefault(t => t.Id == model.StatusTypeId!.Value);

            // Snapshot who currently holds the device BEFORE anything changes.
            var activeSubscription = GetActiveSubscriptionForDevice(model.SimId, model.UsbId);

            bool staysAssigned = string.Equals(statusType?.Name, "Occupied", StringComparison.OrdinalIgnoreCase);
            string? assignedToName = staysAssigned
                ? (activeSubscription?.Employee?.Name ?? activeSubscription?.NonEmployee?.Name)
                : null;

            var deviceStatus = new DeviceStatus
            {
                SimId = model.SimId,
                UsbId = model.UsbId,
                StatusTypeId = model.StatusTypeId!.Value,
                StatusDate = DateTime.Now,
                Notes = model.Notes,
                ReportedBy = currentUserId,
                ReplacedBySimId = model.ReplacedBySimId,
                ReplacedByUsbId = model.ReplacedByUsbId,
                AssignedToName = assignedToName
            };

            _deviceStatusRepo.AddDeviceStatus(deviceStatus);

            ApplyStatusToDevice(model.SimId, model.UsbId, statusType, activeSubscription);

            // If a replacement device was specified, hand it to the same subscriber the
            // original device belonged to: create a fresh subscription for it, mark it
            // Occupied, and log that as its own DeviceStatus entry.
            if (model.ReplacedBySimId.HasValue || model.ReplacedByUsbId.HasValue)
            {
                HandleReplacementDevice(model.ReplacedBySimId, model.ReplacedByUsbId, activeSubscription, currentUserId);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Assigns the replacement SIM/USB to whoever held the original device, opening a
        /// new subscription for it, marking it Occupied, and logging that as a DeviceStatus
        /// entry with AssignedToName set — mirrors how DeviceTransferController and
        /// SubscriptionController log new assignments.
        /// </summary>
        private void HandleReplacementDevice(int? replacedBySimId, int? replacedByUsbId, Subscription? originalSubscription, int reportedBy)
        {
            if (originalSubscription == null) return; // nobody to assign the replacement to

            var occupiedStatusType = _context.DeviceStatusesType
                .FirstOrDefault(t => t.Name == "Occupied");
            if (occupiedStatusType == null) return;

            string? assignedToName = originalSubscription.Employee?.Name ?? originalSubscription.NonEmployee?.Name;

            var newSubscription = new Subscription
            {
                EmpId = originalSubscription.EmpId,
                NonEmployeeId = originalSubscription.NonEmployeeId,
                SimId = replacedBySimId,
                UsbId = replacedByUsbId,
                QuotaId = replacedBySimId.HasValue ? originalSubscription.QuotaId : null,
                ActionId = originalSubscription.ActionId,
                CreatedBy = reportedBy,
                CreatedDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = null,
                Fees = originalSubscription.Fees
            };

            _subscriptionRepo.Add(newSubscription);

            if (replacedBySimId.HasValue)
            {
                var replacementSim = _simRepo.GetById(replacedBySimId.Value);
                if (replacementSim != null)
                {
                    replacementSim.Status = "Occupied";
                    _simRepo.Update(replacementSim);
                }
            }

            if (replacedByUsbId.HasValue)
            {
                var replacementUsb = _usbRepo.GetById(replacedByUsbId.Value);
                if (replacementUsb != null)
                {
                    replacementUsb.Status = "Occupied";
                    _usbRepo.Update(replacementUsb);
                }
            }

            var replacementStatus = new DeviceStatus
            {
                SimId = replacedBySimId,
                UsbId = replacedByUsbId,
                StatusTypeId = occupiedStatusType.Id,
                StatusDate = DateTime.Now,
                Notes = "Assigned as replacement device",
                ReportedBy = reportedBy,
                AssignedToName = assignedToName
            };

            _deviceStatusRepo.AddDeviceStatus(replacementStatus);
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

        private void ApplyStatusToDevice(int? simId, int? usbId, DeviceStatusType? statusType, Subscription? activeSubscription)
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

            // Any status other than "Occupied" means the device is no longer
            // actively assigned, so detach it from whatever subscription
            // currently holds it (already fetched above, before this call).
            if (!string.Equals(statusType.Name, "Occupied", StringComparison.OrdinalIgnoreCase))
            {
                DetachDeviceFromActiveSubscription(simId, usbId, activeSubscription);
            }
        }

        /// <summary>
        /// Removes this device from whatever active subscription currently holds it.
        /// A subscription can hold a SIM and a USB at once, so we only clear the
        /// matching slot — the subscription is fully closed (EndDate set) only once
        /// neither slot is occupied anymore.
        /// </summary>
        private void DetachDeviceFromActiveSubscription(int? simId, int? usbId, Subscription? activeSubscription)
        {
            if (activeSubscription == null) return;

            if (simId.HasValue && activeSubscription.SimId == simId)
            {
                activeSubscription.SimId = null;
            }

            if (usbId.HasValue && activeSubscription.UsbId == usbId)
            {
                activeSubscription.UsbId = null;
            }

            if (activeSubscription.SimId == null && activeSubscription.UsbId == null)
            {
                activeSubscription.EndDate = DateTime.Now;
            }

            _subscriptionRepo.Update(activeSubscription);
        }

        private Subscription? GetActiveSubscriptionForDevice(int? simId, int? usbId)
        {
            return _subscriptionRepo.GetAll()
                .FirstOrDefault(s =>
                    (s.EndDate == null || s.EndDate > DateTime.Now) &&
                    ((simId.HasValue && s.SimId == simId) || (usbId.HasValue && s.UsbId == usbId)));
        }

        private void PopulateLookupLists(DeviceStatusCreateViewModel model)
        {
            var sims = _simRepo.GetAll().ToList();
            var usbs = _usbRepo.GetAll().ToList();

            model.Sims = sims
                .Select(s => new DeviceOptionViewModel { Id = s.Id, SerialNumber = s.SerialNumber, Status = s.Status })
                .ToList();

            model.Usbs = usbs
                .Select(u => new DeviceOptionViewModel { Id = u.Id, SerialNumber = u.SerialNumber, Status = u.Status })
                .ToList();

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