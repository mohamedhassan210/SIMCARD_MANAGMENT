using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.ViewModels;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly INonEmployeeRepo _nonEmployeeRepo;
        private readonly IGroupRepo _groupRepo;

        public EmployeeController(
            IEmployeeRepo employeeRepo,
            INonEmployeeRepo nonEmployeeRepo,
            IGroupRepo groupRepo)
        {
            _employeeRepo = employeeRepo;
            _nonEmployeeRepo = nonEmployeeRepo;
            _groupRepo = groupRepo;
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return View(employee);
            }

            //employee.Id = int.Newint();
            employee.CreatedAt = DateTime.Now;
            employee.IsActive = true;

            _employeeRepo.Add(employee);

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

            // Soft delete: Change IsActive status to false instead of removing the record from DB
            employee.IsActive = false;
            _employeeRepo.Update(employee);

            return RedirectToAction(nameof(Index));
        }
    }
}