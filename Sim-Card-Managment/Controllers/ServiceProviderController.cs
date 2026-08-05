using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repositories;
using Sim_Card_Managment.ViewModels;
using Sim_Card_Managment.Viewmodel;
using System;
using System.Linq;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class ServiceProviderController : Controller
    {
        private readonly IServiceProviderRepository _repo;

        public ServiceProviderController(IServiceProviderRepository repo)
        {
            _repo = repo;
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

        public IActionResult Create() => View(new ServiceProviderViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceProviderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var provider = new Models.ServiceProvider
                {
                    //Id = int.Newint(),
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

            var simsList = provider.Sims.Select(s => new DeviceDirectoryViewModel
            {
                Id = s.Id,
                SerialNumber = s.SerialNumber,
                Identifier = s.PhoneNumber,
                DeviceType = "SIM Card",
                ServiceProvider = provider.Name,
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
                Identifier = "N/A",
                DeviceType = "USB Modem",
                ServiceProvider = provider.Name,
                Status = u.Status,
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

        // GET: ServiceProvider/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var provider = await _repo.GetByIdAsync(id);
            if (provider == null) return NotFound();

            return View(provider);
        }

        // POST: ServiceProvider/Delete/{id}  (soft delete: IsActive -> false)
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
    }
}