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

            var viewModel = BuildDeviceStatusViewModels(statuses);
            viewModel.Reverse();
            return View(viewModel);
        }

        /// <summary>
        /// GET: /DeviceStatus/ExportDeviceStatusExcel
        /// Exports the device status log to Excel, filtered by the same
        /// dropdown filters the Index view offers (Availability, Device Type).
        /// Free-text search is intentionally excluded — it's client-side only.
        /// </summary>
        [HttpGet]
        public IActionResult ExportDeviceStatusExcel(bool? isActive, string? deviceType)
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

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Device Status Log");

                worksheet.Cells[1, 1].Value = "Serial Number";
                worksheet.Cells[1, 2].Value = "Device Type";
                worksheet.Cells[1, 3].Value = "Old Status";
                worksheet.Cells[1, 4].Value = "New Status";
                worksheet.Cells[1, 5].Value = "Availability";
                worksheet.Cells[1, 6].Value = "Assigned To";
                worksheet.Cells[1, 7].Value = "Reported By";
                worksheet.Cells[1, 8].Value = "Status Date";
                worksheet.Cells[1, 9].Value = "Notes";

                using (var range = worksheet.Cells[1, 1, 1, 9])
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
                    worksheet.Cells[row, 3].Value = r.OldStatus;
                    worksheet.Cells[row, 4].Value = r.NewStatus;
                    worksheet.Cells[row, 5].Value = r.Availability;
                    worksheet.Cells[row, 6].Value = r.AssignedTo;
                    worksheet.Cells[row, 7].Value = r.ReportedByUserName;
                    worksheet.Cells[row, 8].Value = r.StatusDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[row, 9].Value = r.Notes ?? "";
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileNameParts = new List<string>();
                if (isActive.HasValue) fileNameParts.Add(isActive.Value ? "Active" : "NotActive");
                if (!string.IsNullOrWhiteSpace(deviceType)) fileNameParts.Add(deviceType.Replace(" ", ""));

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

            var deviceStatus = new DeviceStatus
            {
                SimId = model.SimId,
                UsbId = model.UsbId,
                StatusTypeId = model.StatusTypeId!.Value,
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