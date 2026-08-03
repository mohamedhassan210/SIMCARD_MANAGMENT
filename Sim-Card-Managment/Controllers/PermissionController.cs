using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Services;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;

namespace Sim_Card_Managment.Controllers
{
    /// <summary>
    /// Handles Group Permission Management operations.
    /// Manages assignment of permissions to groups using Repository Pattern.
    /// </summary>
    public class PermissionController : Controller
    {
        private readonly IGroupRepo _groupRepo;
        private readonly IPermissionRepo _permissionRepo;
        private readonly PermissionDiscoveryService _discoveryService;
        private readonly AppDbContext _context;

        public PermissionController(
            IGroupRepo groupRepo,
            IPermissionRepo permissionRepo,
            PermissionDiscoveryService discoveryService,
            AppDbContext context)
        {
            _groupRepo = groupRepo;
            _permissionRepo = permissionRepo;
            _discoveryService = discoveryService;
            _context = context;
        }

        /// <summary>
        /// GET: Permission/Index
        /// Displays all Groups and available Permissions for management.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var groups = (await _groupRepo.GetAllAsync()).ToList();
            var permissions = (await _permissionRepo.GetAllAsync()).ToList();

            var model = new PermissionIndexViewModel
            {
                Groups = groups,
                Permissions = permissions
            };

            return View(model);
        }

        /// <summary>
        /// GET: Permission/AssignPermissions/{groupId}
        /// Displays the permission assignment form for a specific group.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> mangePermission(int groupId)
        {
            var group = await _groupRepo.GetByIdWithPermissionsAsync(groupId);
            if (group == null)
            {
                TempData["ErrorMessage"] = "Group not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get all permissions from database
            var allPermissions = (await _permissionRepo.GetAllAsync()).ToList();

            // Get currently assigned permission IDs for this group
            var assignedPermissionIds = group.GroupPermissions
                .Select(gp => gp.PermissionId)
                .ToHashSet();

            // Build the ViewModel grouped by Controller
            var permissionsByController = allPermissions
                .GroupBy(p => p.ControllerName)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .OrderBy(p => p.ActionName)
                        .Select(p => new PermissionItemViewModel
                        {
                            PermissionId = p.Id,
                            ControllerName = p.ControllerName,
                            ActionName = p.ActionName,
                            Description = p.Description,
                            IsAssigned = assignedPermissionIds.Contains(p.Id)
                        })
                        .ToList()
                );

            var model = new AssignPermissionsViewModel
            {
                GroupId = group.Id,
                GroupName = group.Name,
                PermissionsByController = permissionsByController
            };

            return View(model);
        }

        /// <summary>
        /// POST: Permission/AssignPermissions
        /// Processes the permission assignment form submission.
        /// Updates GroupPermission junction table based on selected permissions.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> mangePermission(
            AssignPermissionsViewModel model,
            [FromForm(Name = "selectedPermissionIds")] List<int> selectedPermissionIds)
        {
            try
            {
                // Verify group exists
                var group = await _groupRepo.GetByIdWithPermissionsAsync(model.GroupId);
                if (group == null)
                {
                    TempData["ErrorMessage"] = "Group not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Null safety for selectedPermissionIds
                selectedPermissionIds ??= new List<int>();

                // Fetch existing GroupPermission records for this group
                var existingPermissions = _context.GroupPermissions
                    .Where(gp => gp.GroupId == model.GroupId)
                    .ToList();

                // Identify permissions to remove (not in selectedPermissionIds)
                var permissionsToRemove = existingPermissions
                    .Where(gp => !selectedPermissionIds.Contains(gp.PermissionId))
                    .ToList();

                // Remove permissions
                if (permissionsToRemove.Any())
                {
                    _context.GroupPermissions.RemoveRange(permissionsToRemove);
                }

                // Identify new permissions to add (in selectedPermissionIds but not already assigned)
                var currentPermissionIds = existingPermissions
                    .Select(gp => gp.PermissionId)
                    .ToHashSet();

                var permissionsToAdd = selectedPermissionIds
                    .Where(id => !currentPermissionIds.Contains(id))
                    .Select(id => new GroupPermission
                    {
                        GroupId = model.GroupId,
                        PermissionId = id
                    })
                    .ToList();

                // Add new permissions
                if (permissionsToAdd.Any())
                {
                    _context.GroupPermissions.AddRange(permissionsToAdd);
                }

                // Save changes asynchronously
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Permissions for group '{group.Name}' have been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating permissions: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: Permission/RefreshPermissions
        /// Executes the PermissionDiscoveryService to scan and seed permissions from controllers.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshPermissions()
        {
            try
            {
                // Execute discovery service to scan and update permissions
                await _discoveryService.SeedPermissionsAsync(_context);

                TempData["SuccessMessage"] = "Permissions have been refreshed and synchronized from available controllers.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while refreshing permissions: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}