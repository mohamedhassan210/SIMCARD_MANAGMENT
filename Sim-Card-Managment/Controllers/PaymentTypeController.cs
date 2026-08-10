using Microsoft.AspNetCore.Mvc;
using Sim_Card_Management.Models;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.LookupRepos;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class PaymentTypeController : Controller
    {
        private readonly ILookupRepo _lookupRepo;

        public PaymentTypeController(ILookupRepo lookupRepo)
        {
            _lookupRepo = lookupRepo;
        }

        // GET: PaymentType/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var paymentTypes = await _lookupRepo.GetPaymentTypesAsync();
            return View(paymentTypes);
        }

        // GET: PaymentType/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PaymentType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "Name is required.");
                return View(new PaymentType());
            }

            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int uid) ? uid : 1;

            await _lookupRepo.AddPaymentTypeAsync(new PaymentType
            {
                Name = Name,
                CreatedById = currentUserId
            });

            TempData["Success"] = "Payment type created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: PaymentType/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var paymentType = await _lookupRepo.GetPaymentTypeByIdAsync(id);

            if (paymentType == null)
                return NotFound();

            return View(paymentType);
        }

        // POST: PaymentType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentType paymentType)
        {
            ModelState.Remove(nameof(paymentType.CreatedBy));
            ModelState.Remove(nameof(paymentType.InternetLines));

            if (!ModelState.IsValid)
            {
                return View(paymentType);
            }

            var existing = await _lookupRepo.GetPaymentTypeByIdAsync(paymentType.Id);

            if (existing == null)
                return NotFound();

            existing.Name = paymentType.Name;

            await _lookupRepo.UpdatePaymentTypeAsync(existing);

            TempData["Success"] = "Payment type updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: PaymentType/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var paymentType = await _lookupRepo.GetPaymentTypeByIdAsync(id);

            if (paymentType == null)
                return NotFound();

            return View(paymentType);
        }

        // POST: PaymentType/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _lookupRepo.DeletePaymentTypeAsync(id);

            TempData["Success"] = "Payment type deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}