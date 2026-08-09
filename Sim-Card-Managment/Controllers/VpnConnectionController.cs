using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.BranchRepos;
using Sim_Card_Managment.Repos.LookupRepos;
using Sim_Card_Managment.Repos.VpnConnectionRepos;
using Sim_Card_Managment.Viewmodel;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class VpnConnectionController : Controller
    {
        private readonly IVpnConnectionRepo _vpnRepo;
        private readonly IBranchRepo _branchRepo;
        private readonly ILookupRepo _lookupRepo;
        private readonly IServiceProviderRepository _serviceProviderRepo;

        public VpnConnectionController(
            IVpnConnectionRepo vpnRepo,
            IBranchRepo branchRepo,
            ILookupRepo lookupRepo,
            IServiceProviderRepository serviceProviderRepo)
        {
            _vpnRepo = vpnRepo;
            _branchRepo = branchRepo;
            _lookupRepo = lookupRepo;
            _serviceProviderRepo = serviceProviderRepo;
        }

        private async Task PopulateDropdowns(VpnConnectionCreateViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var connectionTypes = await _lookupRepo.GetVpnConnectionTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ConnectionTypes = connectionTypes
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
        }

        private async Task PopulateDropdowns(VpnConnectionEditViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var connectionTypes = await _lookupRepo.GetVpnConnectionTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ConnectionTypes = connectionTypes
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vpns = await _vpnRepo.GetAllAsync();
            return View(vpns);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var vpn = await _vpnRepo.GetByIdWithDetailsAsync(id);
            if (vpn == null) return NotFound();
            return View(vpn);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new VpnConnectionCreateViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VpnConnectionCreateViewModel model)
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
            {
                model.CreatedById = currentUserId;
            }
            else
            {
                model.CreatedById = 1; // Default fallback ID if unauthenticated in dev
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            await _vpnRepo.AddAsync(model);

            TempData["Success"] = "VPN connection created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _vpnRepo.GetForEditAsync(id);
            if (model == null) return NotFound();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VpnConnectionEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            await _vpnRepo.UpdateAsync(model);
            TempData["Success"] = "VPN connection updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
    }
}