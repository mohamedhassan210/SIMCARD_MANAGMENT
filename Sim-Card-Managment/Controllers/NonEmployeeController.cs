using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using System;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class NonEmployeeController : Controller
    {
        private readonly INonEmployeeRepo _nonEmployeeRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly AppDbContext _context;

        public NonEmployeeController(INonEmployeeRepo nonEmployeeRepo, IEmployeeRepo employeeRepo, AppDbContext context)
        {
            _nonEmployeeRepo = nonEmployeeRepo;
            _employeeRepo = employeeRepo;
            _context = context;
        }
        // GET: /NonEmployee/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /NonEmployee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NonEmployee nonEmployee)
        {
            // Reject duplicate contact info, if that's your uniqueness rule
            bool contactExists = _context.NonEmployees.Any(n => n.ContactInfo == nonEmployee.ContactInfo);
            if (contactExists)
            {
                ModelState.AddModelError(nameof(nonEmployee.ContactInfo), "A non-employee with this contact info already exists.");
            }

            if (ModelState.IsValid)
            {
                nonEmployee.CreatedAt = DateTime.Now;
                nonEmployee.IsActive = true;
                _nonEmployeeRepo.Add(nonEmployee);
                return RedirectToAction(nameof(Index));
            }

            return View(nonEmployee);
        }
        // GET: /NonEmployee/Details/{id}
        public IActionResult Details(int id)
        {
            // 1. Fetch non-employee by ID
            var nonEmployee = _nonEmployeeRepo.GetById(id);
            if (nonEmployee != null)
            {
                return View("Details", nonEmployee);
            }

            // 2. Fallback: Check if person is an Employee
            var employee = _employeeRepo.GetById(id);
            if (employee != null)
            {
                return RedirectToAction("Details", "Employee", new { id = id });
            }

            return NotFound();
        }
        private void PopulateTypesDropdown()
        {
            // Fetch distinct types via repository
            var existingTypes = _nonEmployeeRepo.GetDistinctTypes();

            // Default categories
            var defaultTypes = new List<string> { "Contractor", "Visitor", "Consultant", "Vendor", "Temporary" };

            // Merge defaults with existing DB types without duplicates
            var allTypes = defaultTypes.Union(existingTypes).Distinct().ToList();

            ViewBag.TypeOptions = allTypes;
        }

        // GET: /NonEmployee/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var nonEmployee = _nonEmployeeRepo.GetById(id);
            if (nonEmployee == null)
            {
                return NotFound();
            }

            PopulateTypesDropdown();
            return View(nonEmployee);
        }

        // POST: /NonEmployee/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NonEmployee nonEmployee, string? CustomType)
        {
            // Handle "SomethingElse" custom type input
            if (nonEmployee.Type == "SomethingElse")
            {
                if (string.IsNullOrWhiteSpace(CustomType))
                {
                    ModelState.AddModelError("Type", "Please specify the custom category type.");
                }
                else
                {
                    nonEmployee.Type = CustomType.Trim();
                }
            }

            if (ModelState.IsValid)
            {
                _nonEmployeeRepo.Update(nonEmployee);
                return RedirectToAction("Details", "NonEmployee", new { id = nonEmployee.Id });
            }

            // Reload dropdown options if model validation fails
            PopulateTypesDropdown();
            return View(nonEmployee);
        }

        // POST: /NonEmployee/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _nonEmployeeRepo.Delete(id);
            return RedirectToAction("Index", "Employee");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Activate(int id)
        {
            var nonEmployee = _nonEmployeeRepo.GetById(id);
            if (nonEmployee == null)
            {
                return NotFound();
            }

            nonEmployee.IsActive = true;
            _nonEmployeeRepo.Update(nonEmployee);

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}