using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.Models;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    public class GroupController : Controller
    {
        private readonly IGroupRepo _groups;
        private readonly IPermissionRepo _permissions;

        public GroupController(IGroupRepo groups, IPermissionRepo permissions)
        {
            _groups = groups;
            _permissions = permissions;
        }

        public async Task<IActionResult> Index()
            => View(await _groups.GetAllAsync());



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
            // Clear navigation properties and auto-set fields from ModelState validation
            ModelState.Remove(nameof(group.CreatedBy));
            ModelState.Remove(nameof(group.Users));
            ModelState.Remove(nameof(group.GroupPermissions));

            // Set logged-in User ID (Adjust key if storing User ID differently)
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

                await _groups.AddAsync(group); // Or _groups.CreateAsync(group) based on your repository method name

                TempData["Success"] = "Group created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(group);
        }


    }
}