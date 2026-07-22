using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Authorization;
using Sim_Card_Managment.Viewmodel;

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

        public IActionResult Index()
        {
            var sims = _simRepo.GetAll().Select(s => new DeviceDirectoryViewModel
            {
                Id = s.Id,
                SerialNumber = s.SerialNumber,
                DeviceType = "SIM",
                ExtraInfo = s.NetworkType,
                Identifier = s.PhoneNumber,
                Status = s.Status,
                AssignedTo = s.Subscriptions
                                   .FirstOrDefault(sub => sub.EndDate == null)
                                   ?.Employee?.Name,
                ServiceProvider = s.ServiceProvider?.Name,
                RegisteredAt = s.RegisteredAt
            });

            var usbs = _usbRepo.GetAll().Select(u => new DeviceDirectoryViewModel
            {
                Id = u.Id,
                SerialNumber = u.SerialNumber,
                DeviceType = "USB",
                ExtraInfo = u.Model,
                Identifier = null,
                Status = u.Status,
                AssignedTo = u.Subscriptions
                                   .FirstOrDefault(sub => sub.EndDate == null)
                                   ?.Employee?.Name,
                ServiceProvider = u.ServiceProvider?.Name,
                RegisteredAt = u.RegisteredAt
            });

            var model = sims.Concat(usbs)
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
        public IActionResult Create(Sim sim)
        {
            if (ModelState.IsValid)
            {
                sim.Id = Guid.NewGuid();
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
            _simRepo.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}