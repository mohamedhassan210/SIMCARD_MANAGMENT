using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.Models;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class GroupController : Controller
    {
        private readonly IGroupRepo _groups;
        private readonly IPermissionRepo _permissions;

        public GroupController(IGroupRepo groups, IPermissionRepo permissions)
        {
            _groups = groups;
            _permissions = permissions;
        }

        // GET: Group/Index
        public async Task<IActionResult> Index()
        {
            var groups = await _groups.GetAllAsync();

            var viewModel = groups.Select(g => new GroupListItemViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                CreatedAt = g.CreatedAt,
                IsActive = g.IsActive,
                EmployeeCount = g.Users?.Count ?? 0
            }).ToList();

            return View(viewModel);
        }

        // GET: Group/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Group/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group group)
        {
            ModelState.Remove(nameof(group.CreatedBy));
            ModelState.Remove(nameof(group.Users));
            ModelState.Remove(nameof(group.GroupPermissions));

            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
            {
                group.CreatedById = currentUserId;
            }
            else
            {
                group.CreatedById = 1; // Default fallback ID if unauthenticated in dev
            }

            if (ModelState.IsValid)
            {
                group.CreatedAt = DateTime.Now;
                group.IsActive = true;
                await _groups.AddAsync(group);
                TempData["Success"] = "Group created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: Group/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var group = await _groups.GetByIdWithDetailsAsync(id);
            if (group == null) return NotFound();

            return View(group);
        }

        // GET: Group/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var group = await _groups.GetByIdAsync(id);
            if (group == null) return NotFound();

            return View(group);
        }

        // POST: Group/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Group group)
        {
            ModelState.Remove(nameof(group.CreatedBy));
            ModelState.Remove(nameof(group.Users));
            ModelState.Remove(nameof(group.GroupPermissions));

            if (!ModelState.IsValid)
            {
                return View(group);
            }

            var existing = await _groups.GetByIdAsync(group.Id);
            if (existing == null) return NotFound();

            // Only update editable fields — CreatedById/CreatedAt stay untouched
            existing.Name = group.Name;
            existing.Description = group.Description;
            existing.IsActive = group.IsActive;

            await _groups.UpdateAsync(existing);
            TempData["Success"] = "Group updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Group/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _groups.GetByIdAsync(id);
            if (group == null) return NotFound();

            return View(group);
        }

        // POST: Group/Delete/{id}  (soft delete: IsActive -> false)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _groups.SoftDeleteAsync(id);
            TempData["Success"] = "Group deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}