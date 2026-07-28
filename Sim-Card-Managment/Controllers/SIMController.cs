using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Authorization;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sim_Card_Managment.Controllers
{
    // [RequirePermission]
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

            if (cleanedPhone.StartsWith("010"))
            {
                targetProviderName = "Vodafone";
            }
            else if (cleanedPhone.StartsWith("012"))
            {
                targetProviderName = "Orange";
            }
            else if (cleanedPhone.StartsWith("015"))
            {
                targetProviderName = "WE";
            }
            else if (cleanedPhone.StartsWith("011"))
            {
                targetProviderName = "Etisalat";
            }
            else
            {
                return null;
            }

            return _context.ServiceProviders
                .FirstOrDefault(sp => sp.Name.ToLower() == targetProviderName.ToLower());
        }

        // GET: /SIM or /SIM/Index
        public IActionResult Index(string status = "all", string type = "all")
        {
            ViewBag.CurrentStatus = status.ToLower();
            ViewBag.CurrentType = type.ToLower();

            // 1. Fetch ALL SIM Cards (Do not pre-filter on server so client-side JS gets all 48 records)
            var simsList = _simRepo.GetAll().Select(s => new DeviceDirectoryViewModel
            {
                Id = s.Id,
                SerialNumber = s.SerialNumber,
                Identifier = s.PhoneNumber,
                DeviceType = "SIM Card",
                ServiceProvider = s.ServiceProvider?.Name ?? "N/A",
                Status = s.Status,
                RegisteredAt = s.RegisteredAt,
                AssignedTo = s.Subscriptions?
                    .Where(sub => sub.EndDate == null || sub.EndDate > DateTime.Now)
                    .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                    .FirstOrDefault() ?? "Unassigned"
            });

            // 2. Fetch ALL USB Modems
            var usbsList = _usbRepo.GetAll().Select(u => new DeviceDirectoryViewModel
            {
                Id = u.Id,
                SerialNumber = u.SerialNumber,
                Identifier = "N/A",
                DeviceType = "USB Modem",
                ServiceProvider = u.ServiceProvider?.Name ?? "N/A",
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

        // GET: /SIM/Details/{id}
        [HttpGet]
        public IActionResult Details(int id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null) return NotFound();

            return View(sim);
        }

        // GET: /SIM/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /SIM/Create
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
                //sim.Id = int.Newint();
                sim.RegisteredAt = DateTime.Now;
                _simRepo.Add(sim);
                return RedirectToAction(nameof(Index));
            }

            return View(sim);
        }

        // GET: /SIM/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null) return NotFound();

            return View(sim);
        }

        // POST: /SIM/Edit/{id}
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

        // GET: /SIM/Delete/{id}
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null) return NotFound();

            return View(sim);
        }

        // POST: /SIM/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null)
            {
                return NotFound();
            }

            sim.Status = "Removed";
            _simRepo.Update(sim);

            return RedirectToAction(nameof(Index));
        }
    }
}