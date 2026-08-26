using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using System.Drawing;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class ServiceProviderController : Controller
    {
        private readonly IServiceProviderRepository _repo;
        private readonly AppDbContext _context;

        public ServiceProviderController(IServiceProviderRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var providers = await _repo.GetAllAsync();
            var model = providers.Select(p => new ServiceProviderViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                IsActive = p.IsActive
            });
            return View(model);
        }

        // GET: ServiceProvider/ExportProvidersExcel?status=active|inactive|all — mirrors Index's Status Filter dropdown.
        [HttpGet]
        public IActionResult ExportProvidersExcel(string status = "all")
        {
            status = (status ?? "all").ToLower();

            var query = _context.ServiceProviders
                .Include(sp => sp.Quotas)
                .Include(sp => sp.Sims)
                .Include(sp => sp.Usbs)
                .AsQueryable();

            if (status == "active") query = query.Where(sp => sp.IsActive);
            else if (status == "inactive") query = query.Where(sp => !sp.IsActive);

            var providers = query.OrderBy(sp => sp.Name).ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Service Providers");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Display Name";
            worksheet.Cells[1, 3].Value = "Status";
            worksheet.Cells[1, 4].Value = "Quota Count";
            worksheet.Cells[1, 5].Value = "SIM Count";
            worksheet.Cells[1, 6].Value = "USB Count";

            using (var headerRange = worksheet.Cells[1, 1, 1, 6])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightSlateGray);
                headerRange.Style.Font.Color.SetColor(Color.White);
            }

            int row = 2;
            foreach (var p in providers)
            {
                worksheet.Cells[row, 1].Value = p.Name;
                worksheet.Cells[row, 2].Value = p.DisplayName ?? "N/A";
                worksheet.Cells[row, 3].Value = p.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 4].Value = p.Quotas?.Count ?? 0;
                worksheet.Cells[row, 5].Value = p.Sims?.Count ?? 0;
                worksheet.Cells[row, 6].Value = p.Usbs?.Count ?? 0;
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();
            var suffix = status != "all" ? "_" + status : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ServiceProviders{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        public IActionResult Create() => View(new ServiceProviderViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceProviderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var provider = new Models.ServiceProvider
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    IsActive = model.IsActive
                };
                await _repo.AddAsync(provider);
                await _repo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: ServiceProvider/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var provider = await _repo.GetByIdWithDevicesAsync(id);
            if (provider == null) return NotFound();

            ViewBag.StatusTypes = _context.DeviceStatusesType
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToList();

            var simsList = provider.Sims.Select(s => new DeviceDirectoryViewModel
            {
                Id = s.Id,
                SerialNumber = s.SerialNumber,
                Identifier = s.PhoneNumber,
                DeviceType = "SIM Card",
                ServiceProvider = provider.Name,
                IsActive = s.IsActive,
                Status = s.Status,
                CurrentStatusType = s.DeviceStatuses
                    .OrderByDescending(ds => ds.StatusDate)
                    .Select(ds => ds.StatusType.Name)
                    .FirstOrDefault(),
                RegisteredAt = s.RegisteredAt,
                AssignedTo = s.Subscriptions?
                    .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                    .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                    .FirstOrDefault() ?? "Unassigned"
            });

            var usbsList = provider.Usbs.Select(u => new DeviceDirectoryViewModel
            {
                Id = u.Id,
                SerialNumber = u.SerialNumber,
                Identifier = u.Model ?? "N/A",
                DeviceType = "USB Modem",
                ServiceProvider = provider.Name,
                IsActive = u.IsActive,
                Status = u.Status,
                CurrentStatusType = u.DeviceStatuses
                    .OrderByDescending(ds => ds.StatusDate)
                    .Select(ds => ds.StatusType.Name)
                    .FirstOrDefault(),
                RegisteredAt = u.RegisteredAt,
                AssignedTo = u.Subscriptions?
                    .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                    .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                    .FirstOrDefault() ?? "Unassigned"
            });

            var model = new ServiceProviderDetailsViewModel
            {
                Id = provider.Id,
                Name = provider.Name,
                DisplayName = provider.DisplayName,
                IsActive = provider.IsActive,
                Quotas = provider.Quotas
                    .Select(q => new QuotaDisplayViewModel
                    {
                        Id = q.Id,
                        BaseAmount = q.BaseAmount,
                        ExtraAmount = q.ExtraAmount,
                        Fees = q.Fees,
                        IsActive = q.IsActive
                    })
                    .OrderByDescending(q => q.IsActive)
                    .ThenBy(q => q.BaseAmount)
                    .ToList(),
                Devices = simsList.Concat(usbsList)
                    .OrderByDescending(d => d.RegisteredAt)
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var provider = await _repo.GetByIdAsync(id);
            if (provider == null) return NotFound();

            var model = new ServiceProviderEditViewModel
            {
                Id = provider.Id,
                Name = provider.Name,
                DisplayName = provider.DisplayName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceProviderEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.DisplayName = model.DisplayName;

            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();

            TempData["Success"] = "Service provider updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var provider = await _repo.GetByIdAsync(id);
            if (provider == null) return NotFound();

            return View(provider);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();
            TempData["Success"] = "Service provider deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            await _repo.ActivateAsync(id);
            TempData["Success"] = "Service provider activated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        // GET: ServiceProvider/ExportProviderDevicesExcel — mirrors the Assigned Devices
        // section's three filters (Availability, Device Type, Status Type), any combination.
        [HttpGet]
        public async Task<IActionResult> ExportProviderDevicesExcel(int providerId, string status = "all", string type = "all", string statusType = "all")
        {
            status = (status ?? "all").ToLower();
            type = (type ?? "all").ToLower();
            statusType = (statusType ?? "all").ToLower();

            var provider = await _repo.GetByIdWithDevicesAsync(providerId);
            if (provider == null) return NotFound();

            var simsList = provider.Sims.Select(s => new DeviceDirectoryViewModel
            {
                Id = s.Id,
                SerialNumber = s.SerialNumber,
                Identifier = s.PhoneNumber,
                DeviceType = "SIM Card",
                ServiceProvider = provider.Name,
                IsActive = s.IsActive,
                Status = s.Status,
                RegisteredAt = s.RegisteredAt,
                AssignedTo = s.Subscriptions?
                    .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                    .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                    .FirstOrDefault() ?? "Unassigned"
            });

            var usbsList = provider.Usbs.Select(u => new DeviceDirectoryViewModel
            {
                Id = u.Id,
                SerialNumber = u.SerialNumber,
                Identifier = u.Model ?? "N/A",
                DeviceType = "USB Modem",
                ServiceProvider = provider.Name,
                IsActive = u.IsActive,
                Status = u.Status,
                RegisteredAt = u.RegisteredAt,
                AssignedTo = u.Subscriptions?
                    .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                    .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                    .FirstOrDefault() ?? "Unassigned"
            });

            var deviceList = simsList.Concat(usbsList).ToList();

            static string NormalizeDeviceType(string? deviceType)
            {
                if (deviceType != null && deviceType.Contains("SIM", StringComparison.OrdinalIgnoreCase)) return "sim";
                if (deviceType != null && deviceType.Contains("USB", StringComparison.OrdinalIgnoreCase)) return "usb";
                return "";
            }

            var filtered = deviceList.Where(d =>
            {
                var normalizedType = NormalizeDeviceType(d.DeviceType);

                bool matchesStatus = status == "all" || (d.IsActive ? "active" : "inactive") == status;
                bool matchesType = type == "all" || normalizedType == type;
                bool matchesStatusType = statusType == "all" || (d.Status?.ToLower() ?? "unassigned") == statusType;

                return matchesStatus && matchesType && matchesStatusType;
            }).ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Devices");

            worksheet.Cells[1, 1].Value = "Serial Number";
            worksheet.Cells[1, 2].Value = "Type";
            worksheet.Cells[1, 3].Value = "Provider";
            worksheet.Cells[1, 4].Value = "Identifier";
            worksheet.Cells[1, 5].Value = "Availability";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Assigned To";

            using (var headerRange = worksheet.Cells[1, 1, 1, 7])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var d in filtered)
            {
                worksheet.Cells[row, 1].Value = d.SerialNumber;
                worksheet.Cells[row, 2].Value = d.DeviceType;
                worksheet.Cells[row, 3].Value = d.ServiceProvider;
                worksheet.Cells[row, 4].Value = string.IsNullOrEmpty(d.Identifier) ? "-" : d.Identifier;
                worksheet.Cells[row, 5].Value = d.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 6].Value = d.Status;
                worksheet.Cells[row, 7].Value = string.IsNullOrEmpty(d.AssignedTo) ? "Unassigned" : d.AssignedTo;
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();

            var suffixParts = new List<string>();
            if (status != "all") suffixParts.Add(status);
            if (type != "all") suffixParts.Add(type);
            if (statusType != "all") suffixParts.Add(statusType);
            var suffix = suffixParts.Any() ? "_" + string.Join("_", suffixParts) : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ServiceProvider_{provider.Name}_Devices{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }
    }
}