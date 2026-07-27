using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Authorization;
using Sim_Card_Managment.Viewmodel;
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

        public SIMController(ISIMRepo simRepo, IUSBRepo usbRepo)
        {
            _simRepo = simRepo;
            _usbRepo = usbRepo;
        }
        public IActionResult Details()
        {
            return View();
        }
        // GET: /SIM or /SIM/Index?status=active&type=all
        public IActionResult Index(string status = "all", string type = "all")
        {
            ViewBag.CurrentStatus = status.ToLower();
            ViewBag.CurrentType = type.ToLower();

            var simsList = Enumerable.Empty<DeviceDirectoryViewModel>();
            var usbsList = Enumerable.Empty<DeviceDirectoryViewModel>();

            // 1. Fetch SIM Cards
            if (type == "all" || type == "sim")
            {
                var query = _simRepo.GetAll();

                if (status != "all")
                {
                    query = query.Where(s => s.Status.ToLower() == status);
                }

                simsList = query.Select(s => new DeviceDirectoryViewModel
                {
                    Id = s.Id,
                    SerialNumber = s.SerialNumber,
                    DeviceType = "SIM",
                    ExtraInfo = s.NetworkType,
                    Identifier = s.PhoneNumber,
                    Status = s.Status,
                    // Safe null-checks using ?. to prevent CS8602 warning
                    AssignedTo = s.Subscriptions
                                    .Where(sub => sub.EndDate == null)
                                    .Select(sub => sub.Employee != null ? sub.Employee.Name : sub.NonEmployee != null ? sub.NonEmployee.Name : null)
                                    .FirstOrDefault(),
                    AssignedToType = s.Subscriptions
                                    .Where(sub => sub.EndDate == null)
                                    .Select(sub => sub.Employee != null ? "Employee" : sub.NonEmployee != null ? "NonEmployee" : null)
                                    .FirstOrDefault(),
                    ServiceProvider = s.ServiceProvider != null ? s.ServiceProvider.Name : null,
                    RegisteredAt = s.RegisteredAt
                });
            }

            // 2. Fetch USB Modems
            if (type == "all" || type == "usb")
            {
                var query = _usbRepo.GetAll();

                if (status != "all")
                {
                    query = query.Where(u => u.Status.ToLower() == status);
                }

                usbsList = query.Select(u => new DeviceDirectoryViewModel
                {
                    Id = u.Id,
                    SerialNumber = u.SerialNumber,
                    DeviceType = "USB",
                    ExtraInfo = u.Model,
                    Identifier = null,
                    Status = u.Status,
                    // Safe null-checks using ?. to prevent CS8602 warning
                    AssignedTo = u.Subscriptions
                                    .Where(sub => sub.EndDate == null)
                                    .Select(sub => sub.Employee != null ? sub.Employee.Name : sub.NonEmployee != null ? sub.NonEmployee.Name : null)
                                    .FirstOrDefault(),
                    AssignedToType = u.Subscriptions
                                    .Where(sub => sub.EndDate == null)
                                    .Select(sub => sub.Employee != null ? "Employee" : sub.NonEmployee != null ? "NonEmployee" : null)
                                    .FirstOrDefault(),
                    ServiceProvider = u.ServiceProvider != null ? u.ServiceProvider.Name : null,
                    RegisteredAt = u.RegisteredAt
                });
            }

            // 3. Combine and Order
            var model = simsList.Concat(usbsList)
                                .OrderBy(d => d.DeviceType)
                                .ThenBy(d => d.SerialNumber)
                                .ToList();

            return View(model);
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
            if (ModelState.IsValid)
            {
                sim.Id = Guid.NewGuid();
                sim.RegisteredAt = DateTime.Now;
                _simRepo.Add(sim);
                return RedirectToAction(nameof(Index));
            }

            return View(sim);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null) return NotFound();
            return View(sim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sim sim)
        {
            if (ModelState.IsValid)
            {
                _simRepo.Update(sim);
                return RedirectToAction(nameof(Index));
            }

            return View(sim);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var sim = _simRepo.GetById(id);
            if (sim == null) return NotFound();
            return View(sim);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _simRepo.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}