using Microsoft.AspNetCore.Mvc;
using Sim_Card_Management.Models;
using Sim_Card_Managment.Repos.LookupRepos;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class ServiceTypeController : Controller
    {
        private readonly ILookupRepo _lookupRepo;

        public ServiceTypeController(ILookupRepo lookupRepo)
        {
            _lookupRepo = lookupRepo;
        }

        // GET: ServiceType/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();
            return View(serviceTypes);
        }

        // GET: ServiceType/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServiceType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceType serviceType)
        {
            ModelState.Remove(nameof(serviceType.InternetLines));

            if (!ModelState.IsValid)
            {
                return View(serviceType);
            }

            await _lookupRepo.AddServiceTypeAsync(serviceType);

            TempData["Success"] = "Service type created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceType/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var serviceType =
                await _lookupRepo.GetServiceTypeByIdAsync(id);

            if (serviceType == null)
                return NotFound();

            return View(serviceType);
        }

        // POST: ServiceType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceType serviceType)
        {
            ModelState.Remove(nameof(serviceType.InternetLines));

            if (!ModelState.IsValid)
            {
                return View(serviceType);
            }

            var existing =
                await _lookupRepo.GetServiceTypeByIdAsync(serviceType.Id);

            if (existing == null)
                return NotFound();

            existing.Name = serviceType.Name;

            await _lookupRepo.UpdateServiceTypeAsync(existing);

            TempData["Success"] = "Service type updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceType/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var serviceType =
                await _lookupRepo.GetServiceTypeByIdAsync(id);

            if (serviceType == null)
                return NotFound();

            return View(serviceType);
        }

        // POST: ServiceType/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _lookupRepo.DeleteServiceTypeAsync(id);

            TempData["Success"] = "Service type deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}