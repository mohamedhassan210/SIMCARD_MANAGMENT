using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Authorization;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using System;
using System.Collections.Generic;
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
                Identifier = "N/A",
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

            var combinedDirectory = simsList.Concat(usbsList)
                                            .OrderByDescending(d => d.RegisteredAt)
                                            .ToList();

            return View(combinedDirectory);
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