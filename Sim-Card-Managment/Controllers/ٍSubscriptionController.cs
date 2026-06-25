using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;

namespace Sim_Card_Managment.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionRepo _subscriptionRepo;
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;
        private readonly IQuotaRepo _quotaRepo;

        public SubscriptionController(
            ISubscriptionRepo subscriptionRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            IQuotaRepo quotaRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _quotaRepo = quotaRepo;
        }

        public IActionResult Index()
        {
            var subscriptions = _subscriptionRepo.GetAll();
            return View(subscriptions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Subscription subscription)
        {
            if (ModelState.IsValid)
            {
                subscription.Id = Guid.NewGuid();

                _subscriptionRepo.Add(subscription);

                TempData["Success"] = "Subscription Added Successfully";
                return RedirectToAction(nameof(Index));
            }

            LoadLists();
            return View(subscription);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var subscription = _subscriptionRepo.GetById(id);

            if (subscription == null)
                return NotFound();

            LoadLists();
            return View(subscription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Subscription subscription)
        {
            if (ModelState.IsValid)
            {
                _subscriptionRepo.Update(subscription);

                TempData["Success"] = "Subscription Updated Successfully";
                return RedirectToAction(nameof(Index));
            }

            LoadLists();
            return View(subscription);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var subscription = _subscriptionRepo.GetById(id);

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _subscriptionRepo.Delete(id);

            TempData["Success"] = "Subscription Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(Guid id)
        {
            var subscription = _subscriptionRepo.GetById(id);

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        private void LoadLists()
        {
            ViewBag.Sims = new SelectList(
                _simRepo.GetAll(),
                "Id",
                "SerialNumber");

            ViewBag.Usbs = new SelectList(
                _usbRepo.GetAll(),
                "Id",
                "SerialNumber");

            ViewBag.Quotas = new SelectList(
                _quotaRepo.GetAll(),
                "Id",
                "BaseAmount");
        }
    }
}