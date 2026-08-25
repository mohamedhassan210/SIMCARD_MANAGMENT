using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.Authorization;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class SIMController : Controller
    {
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;
        private readonly AppDbContext _context;

        public SIMController(ISIMRepo simRepo, IUSBRepo usbRepo, AppDbContext context)
        {
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _context = context;
        }

        private Sim_Card_Managment.Models.ServiceProvider? DetectServiceProvider(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;

            string cleanedPhone = phoneNumber.Trim();
            string targetProviderName = string.Empty;

            if (cleanedPhone.StartsWith("010")) targetProviderName = "Vodafone";
            else if (cleanedPhone.StartsWith("012")) targetProviderName = "Orange";
            else if (cleanedPhone.StartsWith("015")) targetProviderName = "WE";
            else if (cleanedPhone.StartsWith("011")) targetProviderName = "Etisalat";
            else return null;

            return _context.ServiceProviders
                .FirstOrDefault(sp => sp.Name.ToLower() == targetProviderName.ToLower());
        }

        // Shared by Index and ExportDevicesExcel so both always see the same data.
        private List<DeviceDirectoryViewModel> BuildDeviceDirectory()
        {
            var simsList = _simRepo.GetAll().Select(s => new DeviceDirectoryViewModel
            {
                Id = s.Id,
                SerialNumber = s.SerialNumber,
                Identifier = s.PhoneNumber,
                DeviceType = "SIM Card",
                ServiceProvider = s.ServiceProvider?.Name ?? "N/A",
                IsActive = s.IsActive,
                Status = s.Status,
                RegisteredAt = s.RegisteredAt,
                AssignedTo = s.Subscriptions?
                    .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                    .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                    .FirstOrDefault() ?? "Unassigned"
            });
            var usbsList = _usbRepo.GetAll().Select(u => new DeviceDirectoryViewModel
            {
                Id = u.Id,
                SerialNumber = u.SerialNumber,
                Identifier = u.Model ?? "N/A",   // now shows/searches the model instead of a hardcoded "N/A"
                DeviceType = "USB Modem",
                ServiceProvider = u.ServiceProvider?.Name ?? "N/A",
                IsActive = u.IsActive,
                Status = u.Status,
                RegisteredAt = u.RegisteredAt,
                AssignedTo = u.Subscriptions?
                    .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                    .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                    .FirstOrDefault() ?? "Unassigned"
            });

            return simsList.Concat(usbsList)
                            .OrderByDescending(d => d.RegisteredAt)
                            .ToList();
        }

        private static string NormalizeDeviceType(string? deviceType)
        {
            if (deviceType != null && deviceType.Contains("SIM", StringComparison.OrdinalIgnoreCase)) return "sim";
            if (deviceType != null && deviceType.Contains("USB", StringComparison.OrdinalIgnoreCase)) return "usb";
            return "";
        }

        // GET: /SIM or /SIM/Index
        public IActionResult Index(string status = "all", string type = "all")
        {
            ViewBag.CurrentStatus = status.ToLower();
            ViewBag.CurrentType = type.ToLower();

            // Pull every status type defined in the DeviceStatusType lookup table,
            // rather than only the ones currently in use on Sim/Usb rows.
            ViewBag.StatusTypes = _context.DeviceStatusesType
                .Select(t => t.Name.ToString())
                .OrderBy(n => n)
                .ToList();

            var combinedDirectory = BuildDeviceDirectory();

            return View(combinedDirectory);
        }

        // GET: /SIM/ExportDevicesExcel — mirrors the Index page's three filters
        // (Availability, Device Type, Status Type), any combination of them.
        [HttpGet]
        public IActionResult ExportDevicesExcel(string status = "all", string type = "all", string statusType = "all")
        {
            status = (status ?? "all").ToLower();
            type = (type ?? "all").ToLower();
            statusType = (statusType ?? "all").ToLower();

            var deviceList = BuildDeviceDirectory();

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
                $"Devices{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        public IActionResult Details(int id)
        {
            var sim = _context.Sims
                .Include(s => s.ServiceProvider)
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.Employee)
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.NonEmployee)
                .Include(s => s.DeviceStatuses)
                    .ThenInclude(ds => ds.StatusType)
                .FirstOrDefault(s => s.Id == id);

            if (sim == null) return NotFound();

            return View(sim);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sim sim)
        {
            var provider = DetectServiceProvider(sim.PhoneNumber);
            if (provider != null)
            {
                sim.ServiceProviderId = provider.Id;
                sim.ServiceProvider = provider;
            }
            else
            {
                ModelState.AddModelError("PhoneNumber", "Could not detect a valid Service Provider for this phone number prefix.");
            }

            ModelState.Remove(nameof(Sim.ServiceProvider));
            ModelState.Remove(nameof(Sim.ServiceProviderId));

            // Reject duplicate serial numbers
            bool serialExists = _context.Sims.Any(s => s.SerialNumber == sim.SerialNumber);
            if (serialExists)
            {
                ModelState.AddModelError("SerialNumber", "This serial number is already in use by another SIM.");
            }

            if (ModelState.IsValid)
            {
                sim.RegisteredAt = DateTime.Now;
                sim.IsActive = true;
                sim.Status = "Unassigned";
                _simRepo.Add(sim);
                return RedirectToAction(nameof(Index));
            }

            return View(sim);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null) return NotFound();

            return View(sim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sim sim)
        {
            var provider = DetectServiceProvider(sim.PhoneNumber);
            if (provider != null)
            {
                sim.ServiceProviderId = provider.Id;
                sim.ServiceProvider = provider;
            }
            else
            {
                ModelState.AddModelError("PhoneNumber", "Could not detect a valid Service Provider for this phone number prefix.");
            }

            ModelState.Remove(nameof(Sim.ServiceProvider));
            ModelState.Remove(nameof(Sim.ServiceProviderId));

            // Reject duplicate serial numbers, excluding this SIM itself
            bool serialExists = _context.Sims.Any(s => s.SerialNumber == sim.SerialNumber && s.Id != sim.Id);
            if (serialExists)
            {
                ModelState.AddModelError("SerialNumber", "This serial number is already in use by another SIM.");
            }

            if (ModelState.IsValid)
            {
                _simRepo.Update(sim);
                return RedirectToAction(nameof(Index));
            }

            return View(sim);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null) return NotFound();

            return View(sim);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null)
            {
                return NotFound();
            }

            sim.IsActive = false;
            _simRepo.Update(sim);

            return RedirectToAction(nameof(Index));
        }
    }
}