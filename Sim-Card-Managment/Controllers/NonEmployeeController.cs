using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Authorization;
using System;
using Sim_Card_Managment.Repos.NonEmployeeRepos;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class NonEmployeeController : Controller
    {
        private readonly INonEmployeeRepo _repo;

        public NonEmployeeController(INonEmployeeRepo repo)
        {
            _repo = repo;
        }

        // GET: /NonEmployee
        public IActionResult Index()
        {
            var nonEmployees = _repo.GetAll();
            return View(nonEmployees);
        }

        // GET: /NonEmployee/Details/{id}
        public IActionResult Details(Guid id)
        {
            var nonEmployee = _repo.GetById(id);
            if (nonEmployee == null) return NotFound();
            return View(nonEmployee);
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
            if (!ModelState.IsValid) return View(nonEmployee);
            nonEmployee.Id = Guid.NewGuid();
            _repo.Add(nonEmployee);
            return RedirectToAction(nameof(Index));
        }

        // GET: /NonEmployee/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            var nonEmployee = _repo.GetById(id);
            if (nonEmployee == null) return NotFound();
            return View(nonEmployee);
        }

        // POST: /NonEmployee/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, NonEmployee nonEmployee)
        {
            if (id != nonEmployee.Id) return BadRequest();
            if (!ModelState.IsValid) return View(nonEmployee);
            _repo.Update(nonEmployee);
            return RedirectToAction(nameof(Index));
        }

        // GET: /NonEmployee/Delete/{id}
        public IActionResult Delete(Guid id)
        {
            var nonEmployee = _repo.GetById(id);
            if (nonEmployee == null) return NotFound();
            return View(nonEmployee);
        }

        // POST: /NonEmployee/DeleteConfirmed/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _repo.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
