using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;

namespace Sim_Card_Managment.Controllers
{
    public class SerialController : Controller
    {
        private readonly ISerialRepo _serialRepo;
        private readonly IDocumentRepo _documentRepo;
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;

        public SerialController(
            ISerialRepo serialRepo,
            IDocumentRepo documentRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo)
        {
            _serialRepo = serialRepo;
            _documentRepo = documentRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
        }

        public async Task<IActionResult> Index(string? serialNumber, Guid? documentId)
        {
            var serials = await _serialRepo.GetAllAsync(serialNumber, documentId);

            // تحويل البيانات لـ SerialListItemViewModel من أجل عرض نظيف وآمن
            var viewModel = serials.Select(s => new SerialListItemViewModel
            {
                Id = s.Id,
                SerialNumber = s.SerialNumber,
                DocumentNumber = s.Document?.DocumentNumber ?? "بدون مستند",
                DocumentTypeName = s.Document?.DocumentType?.DisplayName ?? "غير محدد",
                SimPhoneNumber = s.Sim?.SerialNumber, // أو PhoneNumber حسب الحقل المتوفر لديك
                UsbModelOrSerial = s.Usb?.SerialNumber,
                CreatedByUserName = s.CreatedBy?.Username ?? "System",
                CreatedDate = s.CreatedDate
            });

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var model = new SerialCreateViewModel();
            await PopulateLookupListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SerialCreateViewModel model)
        {
            // جلب الـ UserId الخاص بالمستخدم الحالي من الـ Session أو الـ Identity
            model.UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            if (ModelState.IsValid)
            {
                // التحقق من فرادة السيريال في النظام
                if (await _serialRepo.ExistsAsync(model.SerialNumber))
                {
                    ModelState.AddModelError("SerialNumber", "رقم السيريال هذا مسجل مسبقاً في النظام!");
                    await PopulateLookupListsAsync(model);
                    return View(model);
                }

                // Mapping من ViewModel إلى الـ Domain Model الأساسي لقاعدة البيانات
                var serial = new Serial
                {
                    Id = Guid.NewGuid(),
                    SerialNumber = model.SerialNumber,
                    DocumentId = model.DocumentId,
                    SimId = model.SimId,
                    UsbId = model.UsbId,
                    UserId = model.UserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _serialRepo.AddAsync(serial);
                await _serialRepo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateLookupListsAsync(model);
            return View(model);
        }

        // ميثود مساعدة لتعبئة الـ 3 قوائم المطلوبة (Documents, SIMs, USBs)
        private async Task PopulateLookupListsAsync(SerialCreateViewModel model)
        {
            model.Documents = new SelectList(await _documentRepo.GetAllAsync(), "Id", "DocumentNumber", model.DocumentId);
            model.Sims = new SelectList(await _simRepo.GetAvailableSimsAsync(), "Id", "SerialNumber", model.SimId);
            model.Usbs = new SelectList(await _usbRepo.GetAvailableUsbsAsync(), "Id", "SerialNumber", model.UsbId);
        }
    }
}