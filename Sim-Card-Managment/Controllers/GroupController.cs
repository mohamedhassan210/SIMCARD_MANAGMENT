using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Repos.GroupRepos;

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

    [HttpGet]
    public async Task<IActionResult> AssignPermissions(Guid id)
    {
        var group = await _groups.GetByIdWithPermissionsAsync(id);
        if (group == null) return NotFound();

        var allPermissions = await _permissions.GetAllAsync();
        var assignedIds = group.GroupPermissions.Select(gp => gp.PermissionId).ToHashSet();

        var vm = new AssignPermissionsViewModel
        {
            GroupId = group.Id,
            GroupName = group.Name,
            Permissions = allPermissions.Select(p => new PermissionCheckboxItem
            {
                Id = p.Id,
                ControllerName = p.ControllerName,
                ActionName = p.ActionName,
                Description = p.Description,
                IsAssigned = assignedIds.Contains(p.Id)
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AssignPermissions(Guid groupId, List<Guid>? selectedPermissions)
    {
        await _groups.AssignPermissionsAsync(groupId, selectedPermissions ?? new List<Guid>());
        TempData["Success"] = "Permissions updated.";
        return RedirectToAction(nameof(Index));
    }
}