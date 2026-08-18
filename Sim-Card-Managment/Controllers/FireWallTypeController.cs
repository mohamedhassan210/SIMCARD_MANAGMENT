using Microsoft.AspNetCore.Mvc;
using Sim_Card_Management.Models;
using Sim_Card_Managment.Repos.FireWallTypeRepos;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class FireWallTypeController : Controller
    {
        private readonly IFireWallTypeRepo _fireWallTypeRepo;

        public FireWallTypeController(IFireWallTypeRepo fireWallTypeRepo)
        {
            _fireWallTypeRepo = fireWallTypeRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var types = await _fireWallTypeRepo.GetAllAsync();
            return View(types);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new FireWallType());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FireWallType model)
        {
            ModelState.Remove(nameof(FireWallType.Branches));

            if (!ModelState.IsValid)
                return View(model);

            await _fireWallTypeRepo.AddAsync(model);

            TempData["Success"] = "Firewall type created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _fireWallTypeRepo.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FireWallType model)
        {
            ModelState.Remove(nameof(FireWallType.Branches));

            if (!ModelState.IsValid)
                return View(model);

            await _fireWallTypeRepo.UpdateAsync(model);

            TempData["Success"] = "Firewall type updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}