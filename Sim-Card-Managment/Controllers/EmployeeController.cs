using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.ViewModels;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.GroupRepos; // 1. √÷›‰« «·‹ using «·Œ«’ »«·‹ Group Repo
using Sim_Card_Managment.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks; // √÷›‰«Â« ⁄‘«‰ «·‹ async/await

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepo _repo;
        private readonly IGroupRepo _groupRepo; // 2. ⁄—›‰« „ €Ì— ··‹ Group Repo Â‰«

        // 3. ﬁ„‰« »Õﬁ‰ «·‹ IGroupRepo œ«Œ· «·‹ Constructor
        public EmployeeController(IEmployeeRepo repo, IGroupRepo groupRepo)
        {
            _repo = repo;
            _groupRepo = groupRepo;
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
            return View();
        }

        // 4. ÕÊ·‰« œ«·… «·‹ GET ·‹ async ⁄‘«‰ «·‹ GroupRepo »Ì‘ €· »‹ Task
        // GET: /Employee/Create
        public async Task<IActionResult> Create()
        {
            // Ã·» «·„Ã„Ê⁄«  „‰ ﬁ«⁄œ… «·»Ì«‰«  „‰ Œ·«· «·—Ì»Ê“Ì Ê—Ì » «⁄ﬂ
            var groupsFromDb = await _groupRepo.GetAllAsync();

            // Ê÷⁄ «·„Ã„Ê⁄«  ›Ì ViewBag ⁄‘«‰ ’›Õ… «·‹ HTML  ﬁ—√Â«
            ViewBag.GroupsList = groupsFromDb.ToList();

            return View();
        }

        // 5.  ⁄œÌ· œ«·… «·‹ POST · ” ﬁ»· «·‹ Group «·„Œ «—…
        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, Guid? SelectedGroupId)
        {
            if (!ModelState.IsValid)
            {
                // ·Ê «·»Ì«‰«  „‘ ﬂ«„·… Ê—Ã⁄‰« ·‰›” «·’›Õ…° »‰ÃÌ» «·‹ Groups  «‰Ì ⁄‘«‰ «·‹ Dropdown „Ì›÷«‘
                var groupsFromDb = await _groupRepo.GetAllAsync();
                ViewBag.GroupsList = groupsFromDb.ToList();
                return View(employee);
            }

            employee.Id = Guid.NewGuid();

            // „·ÕÊŸ…: «·‹ SelectedGroupId ‘«Ì· «·‹ ID » «⁄ «·Ã—Ê» «··Ì «Œ «—Â «·„” Œœ„ 
            //  ﬁœ—Ì Â‰«  —»ÿÌÂ »«·„ÊŸ› √Ê »«·„” Œœ„ Õ”» «·‹ Logic «··Ì „ÿ·Ê» „‰ﬂ.

            _repo.Add(employee);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Employee/Edit/{id}
        public IActionResult Edit()
        {
            return View();
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