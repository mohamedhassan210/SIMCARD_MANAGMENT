using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Authorization;
using System;
using Sim_Card_Managment.Repos.EmployeeRepos;

namespace Sim_Card_Managment.Controllers
{
    //[RequirePermission]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepo _repo;

        public EmployeeController(IEmployeeRepo repo)
        {
            _repo = repo;
        }

        // GET: /Employee
        public IActionResult Index()
        {
            var employees = _repo.GetAll();// ?? new List<Employee>();
            return View(employees);
        }

        // GET: /Employee/Details/{id}
        public IActionResult Details(Guid id)
        {
            var employee = _repo.GetById(id);
            if (employee == null) return NotFound();
            return View(employee);
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
            if (!ModelState.IsValid) return View(employee);
            employee.Id = Guid.NewGuid();
            _repo.Add(employee);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Employee/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            var employee = _repo.GetById(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: /Employee/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, Employee employee)
        {
            if (id != employee.Id) return BadRequest();
            if (!ModelState.IsValid) return View(employee);
            _repo.Update(employee);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Employee/Delete/{id}
        public IActionResult Delete(Guid id)
        {
            var employee = _repo.GetById(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: /Employee/DeleteConfirmed/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _repo.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}