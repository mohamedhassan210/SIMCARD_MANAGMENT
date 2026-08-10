using Microsoft.AspNetCore.Mvc;
using Sim_Card_Management.Models;
using Sim_Card_Managment.Repos.LookupRepos;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class VpnConnectionTypeController : Controller
    {
        private readonly ILookupRepo _lookupRepo;

        public VpnConnectionTypeController(ILookupRepo lookupRepo)
        {
            _lookupRepo = lookupRepo;
        }

        // GET: VpnConnectionType/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var connectionTypes = await _lookupRepo.GetVpnConnectionTypesAsync();
            return View(connectionTypes);
        }

        // GET: VpnConnectionType/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: VpnConnectionType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "Name is required.");
                return View(new VpnConnectionType());
            }

            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int uid) ? uid : 1;

            await _lookupRepo.AddVpnConnectionTypeAsync(new VpnConnectionType
            {
                Name = Name,
                CreatedById = currentUserId
            });

            TempData["Success"] = "VPN connection type created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: VpnConnectionType/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var connectionType =
                await _lookupRepo.GetVpnConnectionTypeByIdAsync(id);

            if (connectionType == null)
                return NotFound();

            return View(connectionType);
        }

        // POST: VpnConnectionType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VpnConnectionType connectionType)
        {
            ModelState.Remove(nameof(connectionType.CreatedBy));
            ModelState.Remove(nameof(connectionType.VpnConnections));

            if (!ModelState.IsValid)
            {
                return View(connectionType);
            }

            var existing =
                await _lookupRepo.GetVpnConnectionTypeByIdAsync(connectionType.Id);

            if (existing == null)
                return NotFound();

            existing.Name = connectionType.Name;

            await _lookupRepo.UpdateVpnConnectionTypeAsync(existing);

            TempData["Success"] = "VPN connection type updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: VpnConnectionType/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var connectionType =
                await _lookupRepo.GetVpnConnectionTypeByIdAsync(id);

            if (connectionType == null)
                return NotFound();

            return View(connectionType);
        }

        // POST: VpnConnectionType/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _lookupRepo.DeleteVpnConnectionTypeAsync(id);

            TempData["Success"] = "VPN connection type deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}