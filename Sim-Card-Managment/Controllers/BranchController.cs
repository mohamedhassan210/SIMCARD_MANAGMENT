using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Repos.BranchRepos;
using Sim_Card_Managment.Viewmodel;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class BranchController : Controller
    {
        private readonly IBranchRepo _branchRepo;

        public BranchController(IBranchRepo branchRepo)
        {
            _branchRepo = branchRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var branches = await _branchRepo.GetAllAsync();
            return View(branches);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var branch = await _branchRepo.GetByIdWithDetailsAsync(id);
            if (branch == null) return NotFound();
            return View(branch);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BranchCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchCreateViewModel model)
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
                return View(model);

            await _branchRepo.AddAsync(model);

            TempData["Success"] = "Branch created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _branchRepo.GetForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BranchEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _branchRepo.UpdateAsync(model);
            TempData["Success"] = "Branch updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _branchRepo.SoftDeleteAsync(id);
            TempData["Success"] = "Branch deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            await _branchRepo.ActivateAsync(id);
            TempData["Success"] = "Branch activated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}