using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using Sim_Card_Managment.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly INonEmployeeRepo _nonEmployeeRepo;
        private readonly IGroupRepo _groupRepo;
        private readonly AppDbContext _context;

        public EmployeeController(
            IEmployeeRepo employeeRepo,
            INonEmployeeRepo nonEmployeeRepo,
            IGroupRepo groupRepo,
            AppDbContext context)
        {
            _employeeRepo = employeeRepo;
            _nonEmployeeRepo = nonEmployeeRepo;
            _groupRepo = groupRepo;
            _context = context;
        }

        // GET: /Employee/Index
        public async Task<IActionResult> Index(string status = "all", string type = "all")
        {
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentType = type;

            var resultList = new List<PersonListItemViewModel>();

            // 1. Fetch Employees (Fetch "all" so client-side JavaScript can filter and count properly)
            if (type == "all" || type == "employee")
            {
                var employees = await _employeeRepo.GetPeopleListAsync("all");
                resultList.AddRange(employees);
            }

            // 2. Fetch Non-Employees (External visitors/contractors)
            if (type == "all" || type == "non-employee")
            {
                var nonEmployees = await _nonEmployeeRepo.GetPeopleListAsync();
                resultList.AddRange(nonEmployees);
            }

            // 3. Combine and Order Alphabetically
            var orderedModel = resultList.OrderBy(x => x.Name).ToList();

            return View(orderedModel);
        }

        // GET: /Employee/Details/{id}
        public IActionResult Details(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee != null)
            {
                return View("Details", employee);
            }

            var nonEmployee = _nonEmployeeRepo.GetById(id);
            if (nonEmployee != null)
            {
                return RedirectToAction("Details", "NonEmployee", new { id = id });
            }

            return NotFound();
        }

        // GET: /Employee/Create
        public async Task<IActionResult> Create()
        {
            var model = new EmployeeUserCreateViewModel();
            await PopulateGroupsAsync(model);
            return View(model);
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeUserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateGroupsAsync(model);
                return View(model);
            }

            // Reject duplicate National ID
            bool nationalIdExists = await _context.Employees.AnyAsync(e => e.NationalID == model.NationalID);
            if (nationalIdExists)
            {
                ModelState.AddModelError(nameof(model.NationalID), "An employee with this National ID already exists.");
                await PopulateGroupsAsync(model);
                return View(model);
            }

            // Reject duplicate Employee Code (if provided)
            if (!string.IsNullOrWhiteSpace(model.EmpCode))
            {
                bool empCodeExists = await _context.Employees.AnyAsync(e => e.EmpCode == model.EmpCode);
                if (empCodeExists)
                {
                    ModelState.AddModelError(nameof(model.EmpCode), "An employee with this Employee Code already exists.");
                    await PopulateGroupsAsync(model);
                    return View(model);
                }
            }

            // Extra check beyond DataAnnotations: username must be free before we commit anything
            if (model.HasAccount)
            {
                bool usernameTaken = await _context.Users.AnyAsync(u => u.Username == model.Username);
                if (usernameTaken)
                {
                    ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
                    await PopulateGroupsAsync(model);
                    return View(model);
                }

                bool emailTaken = await _context.Users.AnyAsync(u => u.Email == model.Email);
                if (emailTaken)
                {
                    ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                    await PopulateGroupsAsync(model);
                    return View(model);
                }
            }

            // 1. Save the Employee row
            var employee = new Employee
            {
                Name = model.Name,
                NationalID = model.NationalID,
                EmpCode = model.EmpCode,
                Position = model.Position,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _employeeRepo.Add(employee);

            // 2. If requested, save an independent User row
            if (model.HasAccount)
            {
                var user = new User
                {
                    Username = model.Username!,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Email = model.Email!,
                    GroupId = model.GroupId!.Value,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Employee/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: /Employee/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _employeeRepo.Update(employee);
                return RedirectToAction("Details", new { id = employee.Id });
            }

            return View(employee);
        }

        // GET: /Employee/Delete/{id}
        public IActionResult Delete(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee == null) return NotFound();
            return View(employee);
        }


        // POST: /Employee/DeleteConfirmed/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.IsActive = false;
            _employeeRepo.Update(employee);

            return RedirectToAction(nameof(Index));
        }
        private async Task PopulateGroupsAsync(EmployeeUserCreateViewModel model)
        {
            var groups = await _groupRepo.GetAllAsync();
            var activeGroups = groups.Where(g => g.IsActive);
            model.Groups = new SelectList(activeGroups, "Id", "Name", model.GroupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Activate(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.IsActive = true;
            _employeeRepo.Update(employee);

            return RedirectToAction(nameof(Details), new { id });
        }

        
        // GET: /Employee/ExportEmployeeExcel
        [HttpGet]
        public async Task<IActionResult> ExportEmployeeExcel(string status = "all", string type = "all")
        {
            var resultList = new List<PersonListItemViewModel>();

            if (type == "all" || type == "employee")
            {
                var employees = await _employeeRepo.GetPeopleListAsync(status);
                resultList.AddRange(employees);
            }

            if (type == "all" || type == "non-employee")
            {
                var nonEmployees = await _nonEmployeeRepo.GetPeopleListAsync();
                if (status == "active")
                    nonEmployees = nonEmployees.Where(x => x.IsActive).ToList();
                else if (status == "inactive")
                    nonEmployees = nonEmployees.Where(x => !x.IsActive).ToList();

                resultList.AddRange(nonEmployees);
            }

            var orderedModel = resultList.OrderBy(x => x.Name).ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Directory");

            string[] headers = { "Name", "Type", "Status", "ID / Contact", "SIMs", "USBs", "Start Date" };
            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1].Value = headers[i];

            using (var headerRange = sheet.Cells[1, 1, 1, headers.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(230, 230, 230));
            }

            int row = 2;
            foreach (var item in orderedModel)
            {
                sheet.Cells[row, 1].Value = item.Name;
                sheet.Cells[row, 2].Value = item.PersonType;
                sheet.Cells[row, 3].Value = item.IsActive ? "Active" : "Inactive";
                sheet.Cells[row, 4].Value = item.Identifier;
                sheet.Cells[row, 5].Value = item.ActiveSimOnlyCount;
                sheet.Cells[row, 6].Value = item.ActiveUsbCount;
                sheet.Cells[row, 7].Value = item.StartDate.ToString("yyyy-MM-dd");
                row++;
            }

            if (sheet.Dimension != null)
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            var bytes = package.GetAsByteArray();

            var suffixParts = new List<string>();
            if (!string.IsNullOrEmpty(status) && status != "all")
                suffixParts.Add(char.ToUpper(status[0]) + status.Substring(1));
            if (!string.IsNullOrEmpty(type) && type != "all")
                suffixParts.Add(type == "employee" ? "Employees" : "NonEmployees");

            string suffix = suffixParts.Count > 0 ? "_" + string.Join("_", suffixParts) : "";
            string fileName = $"Directory{suffix}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

    }
}