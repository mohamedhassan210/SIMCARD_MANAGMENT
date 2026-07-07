using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;


namespace Sim_Card_Managment.Controllers
{
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
            return View(types);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentType documentType)
        {
            if (ModelState.IsValid)
            {
                documentType.Id = Guid.NewGuid();
                await _repo.AddAsync(documentType);
                await _repo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(documentType);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var type = await _repo.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DocumentType documentType)
        {
            if (id != documentType.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                await _repo.UpdateAsync(documentType);
                await _repo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(documentType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}