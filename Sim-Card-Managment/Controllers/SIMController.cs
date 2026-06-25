using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.SIMRepo.SIMRepo;

namespace Sim_Card_Managment.Controllers
{
    public class SIMController : Controller
    {
        private readonly ISIMRepo _simRepo;

        public SIMController(ISIMRepo simRepo)
        {
            _simRepo = simRepo;
        }

        public IActionResult Index()
        {
            var sims = _simRepo.GetAll();
            return View(sims);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Sim sim)
        {
            if (ModelState.IsValid)
            {
                sim.Id = Guid.NewGuid();
                _simRepo.Add(sim);

                return RedirectToAction(nameof(Index));
            }

            return View(sim);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var sim = _simRepo.GetById(id);

            if (sim == null)
                return NotFound();

            return View(sim);
        }

        [HttpPost]
        public IActionResult Edit(Sim sim)
        {
            if (ModelState.IsValid)
            {
                _simRepo.Update(sim);
                return RedirectToAction(nameof(Index));
            }

            return View(sim);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            _simRepo.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}