using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Management.Models;
using Sim_Card_Management.Repos.DocumentDetailsRepos;
using Sim_Card_Management.Repos.ItemTypeRepos;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;
using System.Drawing;
using System.IO;
using System.Security.Claims;
namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class DocumentController : Controller
    {
        private readonly IDocumentRepo _documentRepo;
        private readonly IDocumentTypeRepo _documentTypeRepo;
        private readonly IServiceProviderRepository _serviceProviderRepo;
        private readonly IItemTypeRepo _itemTypeRepo;
        private readonly IDocumentDetailsRepo _documentDetailsRepo;
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;
        private readonly ISerialRepo _serialRepo;
        private readonly ISubscriptionRepo _subscriptionRepo;

        public DocumentController(
            IDocumentRepo documentRepo,
            IDocumentTypeRepo documentTypeRepo,
            IServiceProviderRepository serviceProviderRepo,
            IItemTypeRepo itemTypeRepo,
            IDocumentDetailsRepo documentDetailsRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            ISerialRepo serialRepo,
            ISubscriptionRepo subscriptionRepo)
        {
            _documentRepo = documentRepo;
            _documentTypeRepo = documentTypeRepo;
            _serviceProviderRepo = serviceProviderRepo;
            _itemTypeRepo = itemTypeRepo;
            _documentDetailsRepo = documentDetailsRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _serialRepo = serialRepo;
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? documentTypeId)
        {
            var documents = await _documentRepo.GetAllAsync(searchTerm, documentTypeId);
            ViewBag.DocumentTypes = new SelectList(await _documentTypeRepo.GetAllAsync(), "Id", "DisplayName");
            return View(documents);
        }
        public async Task<IActionResult> InventoryReport(string? searchTerm)
        {
            var subscriptions = await _subscriptionRepo.GetAllWithHardwareDetailsAsync();

            // Filter only active subscriptions (where EndDate is null)
            var activeSubscriptions = subscriptions.Where(s => s.EndDate == null).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                activeSubscriptions = activeSubscriptions.Where(s =>
                    (s.Employee != null && s.Employee.Name.Contains(searchTerm)) ||
                    (s.NonEmployee != null && s.NonEmployee.Name.Contains(searchTerm)) ||
                    (s.Sim != null && (s.Sim.PhoneNumber.Contains(searchTerm) || s.Sim.SerialNumber.Contains(searchTerm))) ||
                    (s.Usb != null && s.Usb.SerialNumber.Contains(searchTerm))
                );
            }

            ViewBag.AllSubscriptions = subscriptions; // Passed to trace historical users
            return View(activeSubscriptions.ToList());
        }
        public IActionResult Details(int id)
        {
            return View();
        }
        #region First Report
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? searchTerm, int? documentTypeId)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            // Fetch documents and document types
            var allDocuments = await _documentRepo.GetAllAsync(null,null);
            var allDocTypes = await _documentTypeRepo.GetAllAsync();

            // Get ItemTypes for SIM and USB
            var simItemType = await _itemTypeRepo.GetByNameAsync("SIM") ?? await _itemTypeRepo.GetByNameAsync("Sim");
            var usbItemType = await _itemTypeRepo.GetByNameAsync("USB") ?? await _itemTypeRepo.GetByNameAsync("Usb");

            // Lookup dictionary for document types fallback
            var docTypeDict = allDocTypes.ToDictionary(dt => dt.Id, dt => dt.DisplayName ?? dt.Name);

            var query = allDocuments.AsQueryable();

            // Apply filters
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

                // Set Header Titles
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
                    // 1. Transaction Type Resolution
                    string transactionType = doc.DocumentType?.DisplayName
                        ?? doc.DocumentType?.Name
                        ?? (doc.DocumenttypeId.HasValue && docTypeDict.TryGetValue(doc.DocumenttypeId.Value, out var typeName) ? typeName : "N/A");

                    // 2. SIM Count Resolution (using stored Quantity & ItemTypeId matching)
                    int simCount = 0;
                    if (doc.DocumentDetails != null && doc.DocumentDetails.Any())
                    {
                        simCount = doc.DocumentDetails
                            .Where(d => (simItemType != null && d.ItemTypeId == simItemType.Id) ||
                                        (d.ItemType != null && string.Equals(d.ItemType.Name, "SIM", StringComparison.OrdinalIgnoreCase)) ||
                                        (d.ItemType != null && string.Equals(d.ItemType.Name, "Sim", StringComparison.OrdinalIgnoreCase)))
                            .Sum(d => d.Quantity > 0 ? d.Quantity : (d.Serials?.Count ?? 0));
                    }

                    // 3. USB Count Resolution (using stored Quantity & ItemTypeId matching)
                    int usbCount = 0;
                    if (doc.DocumentDetails != null && doc.DocumentDetails.Any())
                    {
                        usbCount = doc.DocumentDetails
                            .Where(d => (usbItemType != null && d.ItemTypeId == usbItemType.Id) ||
                                        (d.ItemType != null && string.Equals(d.ItemType.Name, "USB", StringComparison.OrdinalIgnoreCase)) ||
                                        (d.ItemType != null && string.Equals(d.ItemType.Name, "Usb", StringComparison.OrdinalIgnoreCase)))
                            .Sum(d => d.Quantity > 0 ? d.Quantity : (d.Serials?.Count ?? 0));
                    }

                    worksheet.Cells[row, 1].Value = doc.DocumentNumber;
                    worksheet.Cells[row, 2].Value = transactionType;
                    worksheet.Cells[row, 3].Value = doc.ActionDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 4].Value = simCount;
                    worksheet.Cells[row, 5].Value = usbCount;
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
        #region Create Document
        /// <summary>
        /// GET: Document/Create
        /// Initializes the wizard view with empty ViewModel and populated dropdowns.
        /// </summary>
        public async Task<IActionResult> Create()
        {
            var viewModel = new DocumentCreateViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        /// <summary>
        /// POST: Document/Create
        /// Processes the multi-step form submission and persists Document, DocumentDetails, and Serial records.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateViewModel viewModel)
        {
            //if (!ModelState.IsValid)
            //{
            //    await PopulateDropdownsAsync(viewModel);
            //    return View(viewModel);
            //}

            try
            {
                var userId = GetCurrentUserId();

                // Background generation of Document Number (yyyyMMddHHmmss)
                var documentNumber = DateTime.Now.ToString("yyyyMMddHHmmss");
                var actionDate = DateTime.Now;

                var document = new Document
                {
                    DocumenttypeId = viewModel.DocumentTypeId,
                    ServiceProviderId = viewModel.ServiceProviderId!.Value,
                    ActionDate = actionDate,
                    Notes = viewModel.Notes ?? "None",
                    SignatureType = viewModel.SignatureType ?? "None",
                    SignatureData = viewModel.SignatureData ?? "None",
                    DocumentNumber = documentNumber,
                    CreatedAt = DateTime.Now,
                    UserId = userId
                };

                await _documentRepo.AddAsync(document);

                // Ensure ItemType records exist
                var simItemType = await _itemTypeRepo.GetByNameAsync("SIM") ?? new ItemType { Name = "SIM" };
                if (simItemType.Id == 0) await _itemTypeRepo.AddAsync(simItemType);

                var usbItemType = await _itemTypeRepo.GetByNameAsync("USB") ?? new ItemType { Name = "USB" };
                if (usbItemType.Id == 0) await _itemTypeRepo.AddAsync(usbItemType);

                // Process SIM items
                if (viewModel.Sims != null && viewModel.Sims.Any())
                {
                    var simDocumentDetails = new DocumentDetails
                    {
                        DocumentId = document.Id,
                        ItemTypeId = simItemType.Id,
                        Quantity = viewModel.Sims.Count
                    };
                    await _documentDetailsRepo.AddAsync(simDocumentDetails);

                    foreach (var simDto in viewModel.Sims)
                    {
                        var sim = new Sim
                        {
                            SerialNumber = simDto.SerialNumber,
                            PhoneNumber = simDto.PhoneNumber,
                            NetworkType = simDto.NetworkType,
                            IsActive = true,
                            RegisteredAt = DateTime.Now,
                            ServiceProviderId = viewModel.ServiceProviderId.Value
                        };
                        await _simRepo.AddAsync(sim);

                        var serial = new Serial
                        {
                            SerialNumber = simDto.SerialNumber,
                            DocumentDetailsId = simDocumentDetails.Id,
                            SimId = sim.Id,
                            CreatedDate = DateTime.Now,
                            UserId = userId
                        };
                        await _serialRepo.AddAsync(serial);
                    }
                }

                // Process USB items
                if (viewModel.Usbs != null && viewModel.Usbs.Any())
                {
                    var usbDocumentDetails = new DocumentDetails
                    {
                        DocumentId = document.Id,
                        ItemTypeId = usbItemType.Id,
                        Quantity = viewModel.Usbs.Count
                    };
                    await _documentDetailsRepo.AddAsync(usbDocumentDetails);

                    foreach (var usbDto in viewModel.Usbs)
                    {
                        var usb = new Usb
                        {
                            SerialNumber = usbDto.SerialNumber,
                            Model = usbDto.Model,
                            IsActive = true,
                            RegisteredAt = DateTime.Now,
                            ServiceProviderId = viewModel.ServiceProviderId.Value
                        };
                        await _usbRepo.AddAsync(usb);

                        var serial = new Serial
                        {
                            SerialNumber = usbDto.SerialNumber,
                            DocumentDetailsId = usbDocumentDetails.Id,
                            UsbId = usb.Id,
                            CreatedDate = DateTime.Now,
                            UserId = userId
                        };
                        await _serialRepo.AddAsync(serial);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"حدث خطأ أثناء حفظ المستند: {ex.Message}");
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        /// <summary>
        /// GET: Document/CheckSerialNumber
        /// AJAX endpoint for real-time serial number uniqueness validation.
        /// Returns JSON { exists: bool, type: "SIM|USB|UNKNOWN" }
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckSerialNumber(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return Json(new { exists = false, type = "UNKNOWN" });
            }

            var simExists = await _simRepo.GetBySerialNumberAsync(serialNumber);
            if (simExists != null)
            {
                return Json(new { exists = true, type = "SIM" });
            }

            var usbExists = await _usbRepo.GetBySerialNumberAsync(serialNumber);
            if (usbExists != null)
            {
                return Json(new { exists = true, type = "USB" });
            }

            return Json(new { exists = false, type = "UNKNOWN" });
        }
        #endregion
        #region Helper Methods

        /// <summary>
        /// Populates DocumentTypes and ServiceProviders dropdown lists via Repositories.
        /// </summary>
        private async Task PopulateDropdownsAsync(DocumentCreateViewModel viewModel)
        {
            var documentTypes = await _documentTypeRepo.GetAllAsync();
            viewModel.DocumentTypes = documentTypes
                .Select(dt => new SelectListItem
                {
                    Value = dt.Id.ToString(),
                    Text = dt.DisplayName ?? dt.Name
                })
                .ToList();

            var serviceProviders = await _serviceProviderRepo.GetAllAsync();
            viewModel.ServiceProviders = serviceProviders
                .Where(sp => sp.IsActive)
                .Select(sp => new SelectListItem
                {
                    Value = sp.Id.ToString(),
                    Text = sp.DisplayName ?? sp.Name
                })
                .ToList();
        }

        /// <summary>
        /// Extracts the logged-in user ID from Claims.
        /// </summary>
        private int GetCurrentUserId()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claimValue, out var userId) ? userId : 1;
        }

        #endregion


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _documentRepo.DeleteAsync(id);
            await _documentRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}