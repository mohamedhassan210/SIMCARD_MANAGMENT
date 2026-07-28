using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.QuoteRepo;

namespace Sim_Card_Managment.Controllers
{
    public class QuotaController : Controller
    {
        private readonly IQuotaRepo _quotaRepo;

        public QuotaController(IQuotaRepo quotaRepo)
        {
            _quotaRepo = quotaRepo;
        }

        // GET: Quota
        public IActionResult Index()
        {
            var quotas = _quotaRepo.GetAll();
            return View(quotas);
        }

        // GET: Quota/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quota/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Quota quota)
        {
            if (ModelState.IsValid)
            {
                //quota.Id = int.Newint();

                _quotaRepo.Add(quota);

                TempData["Success"] = "Quota added successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(quota);
        }

        // GET: Quota/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var quota = _quotaRepo.GetById(id);

            if (quota == null)
                return NotFound();

            return View(quota);
        }

        // POST: Quota/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Quota quota)
        {
            if (ModelState.IsValid)
            {
                _quotaRepo.Update(quota);

                TempData["Success"] = "Quota updated successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(quota);
        }

        // GET: Quota/Delete/{id}
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var quota = _quotaRepo.GetById(id);

            if (quota == null)
                return NotFound();

            return View(quota);
        }

        // POST: Quota/DeleteConfirmed/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _quotaRepo.Delete(id);

            TempData["Success"] = "Quota deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: Quota/Details/{id}
        public IActionResult Details(int id)
        {
            var quota = _quotaRepo.GetById(id);

            if (quota == null)
                return NotFound();

            return View(quota);
        }
    }
}