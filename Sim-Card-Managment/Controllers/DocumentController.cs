using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;
using ClosedXML.Excel;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
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
        private readonly ISubscriptionRepo _subscriptionRepo;

        public DocumentController(
            IDocumentRepo documentRepo,
            IDocumentTypeRepo typeRepo,
            ISerialRepo serialRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            ISubscriptionRepo subscriptionRepo)
        {
            _documentRepo = documentRepo;
            _typeRepo = typeRepo;
            _serialRepo = serialRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<IActionResult> Index(string? searchTerm, Guid? documentTypeId)
        {
            var documents = await _documentRepo.GetAllAsync(searchTerm, documentTypeId);
            ViewBag.DocumentTypes = new SelectList(await _typeRepo.GetAllAsync(), "Id", "DisplayName");
            return View(documents);
        }

        #region First Report
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? searchTerm, Guid? documentTypeId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            // Fetch documents using your repository pattern
            var allDocuments = await _documentRepo.GetAllAsync(); // Or your custom method that includes relations like GetDocumentsWithDetailsAsync()

            var query = allDocuments.AsQueryable();

            // Apply filters in-memory/on query
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.DocumentNumber.Contains(searchTerm) || (d.Notes != null && d.Notes.Contains(searchTerm)));
            }

            if (documentTypeId.HasValue)
            {
                query = query.Where(d => d.DocumenttypeId == documentTypeId.Value);
            }

            var documents = query.OrderByDescending(d => d.CreatedAt).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Documents Summary");

                // Set English Header Titles
                worksheet.Cells[1, 1].Value = "Document Serial";
                worksheet.Cells[1, 2].Value = "Transaction Type";
                worksheet.Cells[1, 3].Value = "Action Date";
                worksheet.Cells[1, 4].Value = "SIMs Count";
                worksheet.Cells[1, 5].Value = "USBs Count";
                worksheet.Cells[1, 6].Value = "Notes";

                // Format Header Row
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSlateGray);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int row = 2;
                foreach (var doc in documents)
                {
                    worksheet.Cells[row, 1].Value = doc.DocumentNumber;
                    worksheet.Cells[row, 2].Value = doc.DocumentType?.DisplayName ?? "N/A";
                    worksheet.Cells[row, 3].Value = doc.ActionDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 4].Value = doc.Serials?.Count(s => s.SimId != null) ?? 0;
                    worksheet.Cells[row, 5].Value = doc.Serials?.Count(s => s.UsbId != null) ?? 0;
                    worksheet.Cells[row, 6].Value = doc.Notes ?? "";
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Documents_Summary_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
        #endregion
        #region Second Report
        [HttpGet]
        public async Task<IActionResult> ExportInventoryToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            // Fetch all subscription records using your specific repository
            var subscriptions = await _subscriptionRepo.GetAllWithHardwareDetailsAsync(); // Assumes it fetches linked Employee, NonEmployee, Sim, and Usb profiles

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Hardware Lifecycle");

                // Set English Header Titles
                worksheet.Cells[1, 1].Value = "Current Holder Name";
                worksheet.Cells[1, 2].Value = "Account Type";
                worksheet.Cells[1, 3].Value = "Phone Number";
                worksheet.Cells[1, 4].Value = "SIM Serial Number";
                worksheet.Cells[1, 5].Value = "USB Serial Number";
                worksheet.Cells[1, 6].Value = "Previous User";
                worksheet.Cells[1, 7].Value = "Notes";

                // Format Header Row
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.ForestGreen);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int row = 2;
                // Focus purely on active deployments
                var activeSubscriptions = subscriptions.Where(s => s.EndDate == null).ToList();

                foreach (var sub in activeSubscriptions)
                {
                    // 1. Current Holder Name (Stays Arabic as requested)
                    string currentHolder = sub.Employee != null ? sub.Employee.Name : (sub.NonEmployee != null ? sub.NonEmployee.Name : "Unassigned");
                    worksheet.Cells[row, 1].Value = currentHolder;

                    // 2. Account Type (English)
                    string accountType = sub.Employee != null ? "Internal Employee" : $"External ({sub.NonEmployee?.Type ?? "Contractor"})";
                    worksheet.Cells[row, 2].Value = accountType;

                    // 3. Hardware Info
                    worksheet.Cells[row, 3].Value = sub.Sim?.PhoneNumber ?? "N/A";
                    worksheet.Cells[row, 4].Value = sub.Sim?.SerialNumber ?? "N/A";
                    worksheet.Cells[row, 5].Value = sub.Usb?.SerialNumber ?? "N/A";

                    // 4. Trace Previous User (Lookback)
                    var historicalRecord = subscriptions.FirstOrDefault(h => h.SimId == sub.SimId && h.Id != sub.Id && h.EndDate != null);
                    string previousHolder = "";
                    if (historicalRecord != null)
                    {
                        previousHolder = historicalRecord.Employee != null ? historicalRecord.Employee.Name : (historicalRecord.NonEmployee?.Name ?? "");
                    }
                    worksheet.Cells[row, 6].Value = string.IsNullOrEmpty(previousHolder) ? "None (First Owner)" : previousHolder;

                    // 5. Notes (Stored in English from our updated SQL script)
                    worksheet.Cells[row, 7].Value = sub.Notes ?? "";

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Hardware_Lifecycle_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
        #endregion
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