using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.ViewModels;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Sim_Card_Managment.Controllers
{
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
            var employeesFromDb = _repo.GetAll()?.ToList();
            var currentDate = DateTime.Now;

            if (employeesFromDb == null)
            {
                return View(new List<EmployeeIndexViewModel>());
            }

            var viewModelList = employeesFromDb.Select(emp => {
                var activeSubs = emp.Subscriptions?
                    .Where(s => s.StartDate <= currentDate && (s.EndDate == null || s.EndDate > currentDate))
                    .ToList();

                return new EmployeeIndexViewModel
                {
                    Id = emp.Id,
                    Name = emp.Name,
                    NationalID = emp.NationalID,
                    ActiveSimOnlyCount = activeSubs?.Count(s => s.UsbId == null) ?? 0,
                    ActiveUsbCount = activeSubs?.Count(s => s.UsbId != null) ?? 0
                };
            }).ToList();

            return View(viewModelList);
        }

        // GET: /Employee/Details/{id}
        public IActionResult Details(Guid id)
        {
            //var employee = _repo.GetById(id);
            //if (employee == null) return NotFound();
            return View(/*employee*/);
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
        public IActionResult Edit(/*Guid id*/)
        {
            //var employee = _repo.GetById(id);
            //if (employee == null) return NotFound();
            return View(/*employee*/);
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