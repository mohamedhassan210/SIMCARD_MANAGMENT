using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;

namespace Sim_Card_Managment.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepo _documentRepo;
        private readonly IDocumentTypeRepo _typeRepo;
        private readonly ISerialRepo _serialRepo;
        // نفترض وجود هذه المستودعات لجلب القوائم
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;

        public DocumentController(
            IDocumentRepo documentRepo,
            IDocumentTypeRepo typeRepo,
            ISerialRepo serialRepo, 
            ISIMRepo simRepo,
            IUSBRepo usbRepo)
        {
            _documentRepo = documentRepo;
            _typeRepo = typeRepo;
            _serialRepo = serialRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
        }

        public async Task<IActionResult> Index(string? searchTerm, Guid? documentTypeId)
        {
            var documents = await _documentRepo.GetAllAsync(searchTerm, documentTypeId);
            ViewBag.DocumentTypes = new SelectList(await _typeRepo.GetAllAsync(), "Id", "DisplayName");
            return View(documents);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new DocumentCreateViewModel
            {
                // تعبئة القوائم من المستودعات لعرضها في الـ View
                DocumentTypes = new SelectList(await _typeRepo.GetAllAsync(), "Id", "DisplayName"),
                Sims = new SelectList(await _simRepo.GetAvailableSimsAsync(), "Id", "PhoneNumber"), // أو SerialNumber
                Usbs = new SelectList(await _usbRepo.GetAvailableUsbsAsync(), "Id", "SerialNumber")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateViewModel model)
        {
            // جلب معرّف المستخدم الحالي (يتم استبداله بنظام الـ Auth الفعلي لديك)
            var currentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            if (ModelState.IsValid)
            {
                // 1. تحليل السيريالات المدخلة وتنظيفها
                var separators = new[] { ',', '\r', '\n' };
                var serialNumbers = model.DocumentNumber
                    .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Distinct()
                    .ToList();

                // 2. التحقق من عدم تكرار السيريالات في النظام
                foreach (var sn in serialNumbers)
                {
                    if (await _serialRepo.ExistsAsync(sn))
                    {
                        ModelState.AddModelError("DocumentNumber", $"السيريال رقم ({sn}) موجود مسبقاً في النظام!");
                        await PopulateLookupListsAsync(model);
                        return View(model);
                    }
                }

                // 3. نقوم بعمل Mapping من الـ ViewModel إلى الـ Domain Model (Document)
                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    DocumenttypeId = model.DocumentTypeId,
                    ActionDate = model.ActionDate,
                    Notes = model.Notes ?? string.Empty,
                    SignatureType = model.SignatureType ?? string.Empty,
                    SignatureData = model.SignatureData ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UserId = currentUserId,
                    DocumentNumber = model.DocumentNumber
                };

                // 4. بناء كائنات الـ Serial وإضافتها للمستند مع الحقول المطلوبة (Id, DocumentId, UserId)
                foreach (var sn in serialNumbers)
                {
                    document.Serials.Add(new Serial
                    {
                        Id = Guid.NewGuid(),
                        SerialNumber = sn,
                        UserId = currentUserId,
                        CreatedDate = DateTime.UtcNow,
                        DocumentId = document.Id,
                        SimId = model.SelectedSimId, // ربط الـ SIM المختار من القائمة
                        UsbId = model.SelectedUsbId  // ربط الـ USB المختار من القائمة
                    });
                }

                // 5. الحفظ النهائي في قاعدة البيانات
                await _documentRepo.AddAsync(document);
                await _documentRepo.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // في حال فشل الـ Validation يتم إعادة بناء القوائم المنسدلة وإرجاع الفيو
            await PopulateLookupListsAsync(model);
            return View(model);
        }

        // ميثود مساعدة لإعادة تعبئة القوائم في حال حدوث خطأ لمنع الـ NullReferenceException
        private async Task PopulateLookupListsAsync(DocumentCreateViewModel model)
        {
            model.DocumentTypes = new SelectList(await _typeRepo.GetAllAsync(), "Id", "DisplayName", model.DocumentTypeId);
            model.Sims = new SelectList(await _simRepo.GetAvailableSimsAsync(), "Id", "PhoneNumber", model.SelectedSimId);
            model.Usbs = new SelectList(await _usbRepo.GetAvailableUsbsAsync(), "Id", "SerialNumber", model.SelectedUsbId);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _documentRepo.DeleteAsync(id);
            await _documentRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}