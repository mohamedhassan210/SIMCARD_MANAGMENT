using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Viewmodel;
using System.Drawing;
using System.IO;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class GroupController : Controller
    {
        private readonly IGroupRepo _groups;
        private readonly IPermissionRepo _permissions;
        private readonly IAccountRepo _accountRepo;
        private readonly AppDbContext _context;

        public GroupController(IGroupRepo groups, IPermissionRepo permissions, IAccountRepo accountRepo, AppDbContext context)
        {
            _groups = groups;
            _permissions = permissions;
            _accountRepo = accountRepo;
            _context = context;
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

        // GET: Group/ExportGroupsExcel
        [HttpGet]
        public async Task<IActionResult> ExportGroupsExcel(bool? isActive)
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
            });

            if (isActive.HasValue)
            {
                viewModel = viewModel.Where(g => g.IsActive == isActive.Value);
            }

            var rows = viewModel.ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Groups");

            worksheet.Cells[1, 1].Value = "Group Name";
            worksheet.Cells[1, 2].Value = "Description";
            worksheet.Cells[1, 3].Value = "Created At";
            worksheet.Cells[1, 4].Value = "Status";
            worksheet.Cells[1, 5].Value = "Employees";

            using (var headerRange = worksheet.Cells[1, 1, 1, 5])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var group in rows)
            {
                worksheet.Cells[row, 1].Value = group.Name;
                worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(group.Description) ? "No description provided." : group.Description;
                worksheet.Cells[row, 3].Value = group.CreatedAt.ToString("MMM dd, yyyy");
                worksheet.Cells[row, 4].Value = group.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 5].Value = group.EmployeeCount;
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();

            var suffix = isActive == true ? "_Active" : isActive == false ? "_Inactive" : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Groups{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        // GET: Group/ExportGroupUsersExcel — the Assigned Users table on Group/Details,
        // filtered by the same Active/Inactive dropdown shown on that page.
        [HttpGet]
        [RequirePermission]
        public async Task<IActionResult> ExportGroupUsersExcel(int groupId, bool? isActive)
        {
            var group = await _groups.GetByIdWithDetailsAsync(groupId);
            if (group == null) return NotFound();

            var users = group.Users.AsEnumerable();
            if (isActive.HasValue)
            {
                users = users.Where(u => u.IsActive == isActive.Value);
            }

            var rows = users.ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Group Users");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "Status";

            using (var headerRange = worksheet.Cells[1, 1, 1, 3])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var user in rows)
            {
                worksheet.Cells[row, 1].Value = user.Username;
                worksheet.Cells[row, 2].Value = user.Email;
                worksheet.Cells[row, 3].Value = user.IsActive ? "Active" : "Inactive";
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();

            var suffix = isActive == true ? "_Active" : isActive == false ? "_Inactive" : "";
            var safeGroupName = string.Concat(group.Name.Split(Path.GetInvalidFileNameChars()));

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{safeGroupName}_Users{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
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

            // Reject duplicate group names
            bool nameExists = await _context.Groups.AnyAsync(g => g.Name == group.Name);
            if (nameExists)
            {
                ModelState.AddModelError(nameof(group.Name), "A group with this name already exists.");
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


        // GET: Group/GetAllGroups — returns groups as JSON for the Swal dropdown
        [HttpGet]
        public async Task<IActionResult> GetAllGroups(int? excludeGroupId = null)
        {
            var groups = await _groups.GetAllAsync();
            var result = groups
                .Where(g => g.IsActive && (!excludeGroupId.HasValue || g.Id != excludeGroupId.Value))
                .Select(g => new { g.Id, g.Name });
            return Json(result);
        }

        // POST: Group/ChangeUserGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserGroup(int userId, int newGroupId)
        {
            var result = await _accountRepo.ChangeUserGroupAsync(userId, newGroupId);
            if (!result)
                return Json(new { success = false, message = "Failed to change group." });

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            await _groups.ActivateAsync(id);
            TempData["Success"] = "Group activated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}