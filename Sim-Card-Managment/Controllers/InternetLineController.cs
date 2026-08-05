using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Managment.Repos.InternetLineRepos;
using Sim_Card_Managment.Repos.BranchRepos;
using Sim_Card_Managment.Repos.LookupRepos;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class InternetLineController : Controller
    {
        private readonly IInternetLineRepo _internetLineRepo;
        private readonly IBranchRepo _branchRepo;
        private readonly ILookupRepo _lookupRepo;
        private readonly IServiceProviderRepository _serviceProviderRepo;

        public InternetLineController(
            IInternetLineRepo internetLineRepo,
            IBranchRepo branchRepo,
            ILookupRepo lookupRepo,
            IServiceProviderRepository serviceProviderRepo)
        {
            _internetLineRepo = internetLineRepo;
            _branchRepo = branchRepo;
            _lookupRepo = lookupRepo;
            _serviceProviderRepo = serviceProviderRepo;
        }

        private async Task PopulateDropdowns(InternetLineCreateViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var paymentTypes = await _lookupRepo.GetPaymentTypesAsync();
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.PaymentTypes = paymentTypes
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ServiceTypes = serviceTypes
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
        }

        private async Task PopulateDropdowns(InternetLineEditViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var paymentTypes = await _lookupRepo.GetPaymentTypesAsync();
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.PaymentTypes = paymentTypes
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ServiceTypes = serviceTypes
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lines = await _internetLineRepo.GetAllAsync();
            return View(lines);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var line = await _internetLineRepo.GetByIdWithDetailsAsync(id);
            if (line == null) return NotFound();
            return View(line);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new InternetLineCreateViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InternetLineCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            await _internetLineRepo.AddAsync(model);
            TempData["Success"] = "Internet line created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _internetLineRepo.GetForEditAsync(id);
            if (model == null) return NotFound();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InternetLineEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            await _internetLineRepo.UpdateAsync(model);
            TempData["Success"] = "Internet line updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
    }
}