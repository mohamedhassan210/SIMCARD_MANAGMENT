using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.MailConfigurationRepos;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class MailConfigurationController : Controller
    {
        private readonly IMailConfigurationRepo _mailConfigurationRepo;

        public MailConfigurationController(IMailConfigurationRepo mailConfigurationRepo)
        {
            _mailConfigurationRepo = mailConfigurationRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var configs = await _mailConfigurationRepo.GetAllAsync();
            return View(configs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MailConfiguration { EnableSsl = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MailConfiguration model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _mailConfigurationRepo.AddAsync(model);

            TempData["Success"] = "Mail configuration created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var config = await _mailConfigurationRepo.GetByIdAsync(id);
            if (config == null) return NotFound();

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MailConfiguration model)
        {
            // Password is optional on Edit (blank = keep existing), so it can't
            // carry a [Required] failure here even though the model has it.
            ModelState.Remove(nameof(MailConfiguration.SenderPassword));

            if (!ModelState.IsValid)
                return View(model);

            await _mailConfigurationRepo.UpdateAsync(model);

            TempData["Success"] = "Mail configuration updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _mailConfigurationRepo.DeleteAsync(id);
            TempData["Success"] = "Mail configuration deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int id)
        {
            await _mailConfigurationRepo.SetActiveAsync(id);
            TempData["Success"] = "Active mail configuration updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}