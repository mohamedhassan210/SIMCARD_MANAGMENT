using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Repos;


namespace Sim_Card_Managment.Controllers
{
    public class SerialController : Controller
    {
        private readonly ISerialRepo _serialRepo;

        public SerialController(ISerialRepo serialRepo)
        {
            _serialRepo = serialRepo;
        }

        public async Task<IActionResult> Index(string? serialNumber, Guid? documentId)
        {
            var serials = await _serialRepo.GetAllAsync(serialNumber, documentId);
            return View(serials);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var serial = await _serialRepo.GetByIdAsync(id);
            if (serial == null) return NotFound();
            return View(serial);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _serialRepo.DeleteAsync(id);
            await _serialRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}