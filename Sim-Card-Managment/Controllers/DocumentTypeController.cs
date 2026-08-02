using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class DocumentTypeController : Controller
    {
        private readonly IDocumentTypeRepo _repo;

        public DocumentTypeController(IDocumentTypeRepo repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var types = await _repo.GetAllAsync();
            // تحويل القائمة لـ ViewModel للعرض
            var viewModel = types.Select(t => new DocumentTypeViewModel
            {
                Id = t.Id,
                Name = t.Name,
                DisplayName = t.DisplayName
            });
            return View(viewModel);
        }

        public IActionResult Create() => View(new DocumentTypeViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Mapping من ViewModel لـ Domain Model
                var documentType = new DocumentType
                {
                    //Id = int.Newint(),
                    Name = model.Name,
                    DisplayName = model.DisplayName
                };

                await _repo.AddAsync(documentType);
                await _repo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var type = await _repo.GetByIdAsync(id);
            if (type == null) return NotFound();

            var model = new DocumentTypeViewModel
            {
                Id = type.Id,
                Name = type.Name,
                DisplayName = type.DisplayName
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentTypeViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var type = await _repo.GetByIdAsync(id);
                if (type == null) return NotFound();

                type.Name = model.Name;
                type.DisplayName = model.DisplayName;

                await _repo.UpdateAsync(type);
                await _repo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}