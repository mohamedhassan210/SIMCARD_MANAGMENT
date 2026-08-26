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
        private readonly AppDbContext _context;

        public DocumentController(
            IDocumentRepo documentRepo,
            IDocumentTypeRepo documentTypeRepo,
            IServiceProviderRepository serviceProviderRepo,
            IItemTypeRepo itemTypeRepo,
            IDocumentDetailsRepo documentDetailsRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            ISerialRepo serialRepo,
            ISubscriptionRepo subscriptionRepo,
            AppDbContext context)
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
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? documentTypeId)
        {
            var documents = await _documentRepo.GetAllAsync(searchTerm, documentTypeId);
            ViewBag.DocumentTypes = new SelectList(await _documentTypeRepo.GetAllAsync(), "Id", "DisplayName", documentTypeId);
            return View(documents);
        }

        public async Task<IActionResult> InventoryReport(string? searchTerm)
        {
            var subscriptions = await _subscriptionRepo.GetAllWithHardwareDetailsAsync();

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

            ViewBag.AllSubscriptions = subscriptions;
            return View(activeSubscriptions.ToList());
        }

        private async Task<(Document? document, List<SimDetailViewModel> sims, List<UsbDetailViewModel> usbs)> BuildDocumentDeviceListsAsync(int id)
        {
            var document = await _documentRepo.GetByIdAsync(id);
            if (document == null)
            {
                return (null, new List<SimDetailViewModel>(), new List<UsbDetailViewModel>());
            }

            var serials = await _serialRepo.GetAllAsync(null, id);
            var now = DateTime.Now;

            var sims = serials
                .Where(s => s.Sim != null)
                .Select(s => new SimDetailViewModel
                {
                    Id = s.Sim.Id,
                    PhoneNumber = s.Sim.PhoneNumber,
                    SerialNumber = s.Sim.SerialNumber,
                    Status = s.Sim.Status,
                    IsActive = s.Sim.IsActive,
                    ProviderName = s.Sim.ServiceProvider?.Name,
                    AssignedTo = s.Sim.Subscriptions?
                        .Where(sub => sub.EndDate == null || sub.EndDate > now)
                        .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                        .FirstOrDefault()
                })
                .ToList();

            var usbs = serials
                .Where(s => s.Usb != null)
                .Select(s => new UsbDetailViewModel
                {
                    Id = s.Usb.Id,
                    SerialNumber = s.Usb.SerialNumber,
                    Model = s.Usb.Model,
                    Status = s.Usb.Status,
                    IsActive = s.Usb.IsActive,
                    ProviderName = s.Usb.ServiceProvider?.Name,
                    AssignedTo = s.Usb.Subscriptions?
                        .Where(sub => sub.EndDate == null || sub.EndDate > now)
                        .Select(sub => sub.Employee?.Name ?? sub.NonEmployee?.Name)
                        .FirstOrDefault()
                })
                .ToList();

            return (document, sims, usbs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var (document, sims, usbs) = await BuildDocumentDeviceListsAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Same lookup source SIM/Index uses, for the Status Type filter dropdowns
            ViewBag.StatusTypes = _context.DeviceStatusesType
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToList();

            var viewModel = new DocumentDetailsViewModel
            {
                DocumentId = document.Id,
                DocumentNumber = document.DocumentNumber,
                DocumentTypeName = document.DocumentType?.DisplayName ?? document.DocumentType?.Name ?? "N/A",
                ActionDate = document.ActionDate,
                CreatedAt = document.CreatedAt,
                Notes = document.Notes,
                Sims = sims,
                Usbs = usbs
            };

            return View(viewModel);
        }

        // GET: /Document/ExportDocumentSimsExcel — mirrors the SIM table's two filters
        // (Availability, Status Type), any combination of them.
        [HttpGet]
        public async Task<IActionResult> ExportDocumentSimsExcel(int documentId, string availability = "all", string statusType = "all")
        {
            availability = (availability ?? "all").ToLower();
            statusType = (statusType ?? "all").ToLower();

            var (document, sims, _) = await BuildDocumentDeviceListsAsync(documentId);
            if (document == null) return NotFound();

            var filtered = sims.Where(s =>
            {
                bool matchesAvailability = availability == "all" || (s.IsActive ? "active" : "inactive") == availability;
                bool matchesStatusType = statusType == "all" || (s.Status?.ToLower() ?? "unassigned") == statusType;
                return matchesAvailability && matchesStatusType;
            }).ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Devices");

            worksheet.Cells[1, 1].Value = "Serial Number";
            worksheet.Cells[1, 2].Value = "Type";
            worksheet.Cells[1, 3].Value = "Provider";
            worksheet.Cells[1, 4].Value = "Identifier";
            worksheet.Cells[1, 5].Value = "Availability";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Assigned To";

            using (var headerRange = worksheet.Cells[1, 1, 1, 7])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var s in filtered)
            {
                worksheet.Cells[row, 1].Value = s.SerialNumber;
                worksheet.Cells[row, 2].Value = "SIM Card";
                worksheet.Cells[row, 3].Value = s.ProviderName;
                worksheet.Cells[row, 4].Value = string.IsNullOrEmpty(s.PhoneNumber) ? "-" : s.PhoneNumber;
                worksheet.Cells[row, 5].Value = s.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 6].Value = s.Status;
                worksheet.Cells[row, 7].Value = string.IsNullOrEmpty(s.AssignedTo) ? "Unassigned" : s.AssignedTo;
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();

            var suffixParts = new List<string>();
            if (availability != "all") suffixParts.Add(availability);
            if (statusType != "all") suffixParts.Add(statusType);
            var suffix = suffixParts.Any() ? "_" + string.Join("_", suffixParts) : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Document_{document.DocumentNumber}_Sims{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        // GET: /Document/ExportDocumentUsbsExcel — mirrors the USB table's two filters
        // (Availability, Status Type), any combination of them.
        [HttpGet]
        public async Task<IActionResult> ExportDocumentUsbsExcel(int documentId, string availability = "all", string statusType = "all")
        {
            availability = (availability ?? "all").ToLower();
            statusType = (statusType ?? "all").ToLower();

            var (document, _, usbs) = await BuildDocumentDeviceListsAsync(documentId);
            if (document == null) return NotFound();

            var filtered = usbs.Where(u =>
            {
                bool matchesAvailability = availability == "all" || (u.IsActive ? "active" : "inactive") == availability;
                bool matchesStatusType = statusType == "all" || (u.Status?.ToLower() ?? "unassigned") == statusType;
                return matchesAvailability && matchesStatusType;
            }).ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Devices");

            worksheet.Cells[1, 1].Value = "Serial Number";
            worksheet.Cells[1, 2].Value = "Type";
            worksheet.Cells[1, 3].Value = "Provider";
            worksheet.Cells[1, 4].Value = "Identifier";
            worksheet.Cells[1, 5].Value = "Availability";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Assigned To";

            using (var headerRange = worksheet.Cells[1, 1, 1, 7])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var u in filtered)
            {
                worksheet.Cells[row, 1].Value = u.SerialNumber;
                worksheet.Cells[row, 2].Value = "USB Modem";
                worksheet.Cells[row, 3].Value = u.ProviderName;
                worksheet.Cells[row, 4].Value = string.IsNullOrEmpty(u.Model) ? "-" : u.Model;
                worksheet.Cells[row, 5].Value = u.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 6].Value = u.Status;
                worksheet.Cells[row, 7].Value = string.IsNullOrEmpty(u.AssignedTo) ? "Unassigned" : u.AssignedTo;
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();

            var suffixParts = new List<string>();
            if (availability != "all") suffixParts.Add(availability);
            if (statusType != "all") suffixParts.Add(statusType);
            var suffix = suffixParts.Any() ? "_" + string.Join("_", suffixParts) : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Document_{document.DocumentNumber}_Usbs{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        #region First Report
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? searchTerm, int? documentTypeId, DateTime? from, DateTime? to)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            var allDocuments = await _documentRepo.GetAllAsync(null, null);
            var allDocTypes = await _documentTypeRepo.GetAllAsync();

            var simItemType = await _itemTypeRepo.GetByNameAsync("SIM") ?? await _itemTypeRepo.GetByNameAsync("Sim");
            var usbItemType = await _itemTypeRepo.GetByNameAsync("USB") ?? await _itemTypeRepo.GetByNameAsync("Usb");

            var docTypeDict = allDocTypes.ToDictionary(dt => dt.Id, dt => dt.DisplayName ?? dt.Name);

            var query = allDocuments.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.DocumentNumber.Contains(searchTerm) || (d.Notes != null && d.Notes.Contains(searchTerm)));
            }

            if (documentTypeId.HasValue)
            {
                query = query.Where(d => d.DocumenttypeId == documentTypeId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(d => d.ActionDate.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                // Inclusive of the whole "to" day.
                query = query.Where(d => d.ActionDate.Date <= to.Value.Date);
            }

            var documents = query.OrderByDescending(d => d.CreatedAt).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Documents Summary");

                worksheet.Cells[1, 1].Value = "Document Serial";
                worksheet.Cells[1, 2].Value = "Transaction Type";
                worksheet.Cells[1, 3].Value = "Action Date";
                worksheet.Cells[1, 4].Value = "SIMs Count";
                worksheet.Cells[1, 5].Value = "USBs Count";
                worksheet.Cells[1, 6].Value = "Notes";

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
                    string transactionType = doc.DocumentType?.DisplayName
                        ?? doc.DocumentType?.Name
                        ?? (doc.DocumenttypeId.HasValue && docTypeDict.TryGetValue(doc.DocumenttypeId.Value, out var typeName) ? typeName : "N/A");

                    int simCount = 0;
                    if (doc.DocumentDetails != null && doc.DocumentDetails.Any())
                    {
                        simCount = doc.DocumentDetails
                            .Where(d => (simItemType != null && d.ItemTypeId == simItemType.Id) ||
                                        (d.ItemType != null && string.Equals(d.ItemType.Name, "SIM", StringComparison.OrdinalIgnoreCase)) ||
                                        (d.ItemType != null && string.Equals(d.ItemType.Name, "Sim", StringComparison.OrdinalIgnoreCase)))
                            .Sum(d => d.Quantity > 0 ? d.Quantity : (d.Serials?.Count ?? 0));
                    }

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

                var fileNameParts = new List<string>();
                if (from.HasValue || to.HasValue)
                {
                    var fromPart = from.HasValue ? from.Value.ToString("yyyyMMdd") : "Start";
                    var toPart = to.HasValue ? to.Value.ToString("yyyyMMdd") : "Now";
                    fileNameParts.Add($"{fromPart}-{toPart}");
                }
                var suffix = fileNameParts.Any() ? "_" + string.Join("_", fileNameParts) : "";

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Documents_Summary{suffix}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
        #endregion
        #region Second Report
        [HttpGet]
        public async Task<IActionResult> ExportInventoryToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            var subscriptions = await _subscriptionRepo.GetAllWithHardwareDetailsAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Hardware Lifecycle");

                worksheet.Cells[1, 1].Value = "Current Holder Name";
                worksheet.Cells[1, 2].Value = "Account Type";
                worksheet.Cells[1, 3].Value = "Phone Number";
                worksheet.Cells[1, 4].Value = "SIM Serial Number";
                worksheet.Cells[1, 5].Value = "USB Serial Number";
                worksheet.Cells[1, 6].Value = "Previous User";
                worksheet.Cells[1, 7].Value = "Notes";

                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.ForestGreen);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int row = 2;
                var activeSubscriptions = subscriptions.Where(s => s.EndDate == null).ToList();

                foreach (var sub in activeSubscriptions)
                {
                    string currentHolder = sub.Employee != null ? sub.Employee.Name : (sub.NonEmployee != null ? sub.NonEmployee.Name : "Unassigned");
                    worksheet.Cells[row, 1].Value = currentHolder;

                    string accountType = sub.Employee != null ? "Internal Employee" : $"External ({sub.NonEmployee?.Type ?? "Contractor"})";
                    worksheet.Cells[row, 2].Value = accountType;

                    worksheet.Cells[row, 3].Value = sub.Sim?.PhoneNumber ?? "N/A";
                    worksheet.Cells[row, 4].Value = sub.Sim?.SerialNumber ?? "N/A";
                    worksheet.Cells[row, 5].Value = sub.Usb?.SerialNumber ?? "N/A";

                    var historicalRecord = subscriptions.FirstOrDefault(h => h.SimId == sub.SimId && h.Id != sub.Id && h.EndDate != null);
                    string previousHolder = "";
                    if (historicalRecord != null)
                    {
                        previousHolder = historicalRecord.Employee != null ? historicalRecord.Employee.Name : (historicalRecord.NonEmployee?.Name ?? "");
                    }
                    worksheet.Cells[row, 6].Value = string.IsNullOrEmpty(previousHolder) ? "None (First Owner)" : previousHolder;

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
        public async Task<IActionResult> Create()
        {
            var viewModel = new DocumentCreateViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateViewModel viewModel)
        {
            try
            {
                var userId = GetCurrentUserId();

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

                var simItemType = await _itemTypeRepo.GetByNameAsync("SIM") ?? new ItemType { Name = "SIM" };
                if (simItemType.Id == 0) await _itemTypeRepo.AddAsync(simItemType);

                var usbItemType = await _itemTypeRepo.GetByNameAsync("USB") ?? new ItemType { Name = "USB" };
                if (usbItemType.Id == 0) await _itemTypeRepo.AddAsync(usbItemType);

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
                        if (string.IsNullOrWhiteSpace(simDto.PhoneNumber) ||
                            simDto.PhoneNumber.Length != 11 ||
                            !simDto.PhoneNumber.StartsWith("01") ||
                            !simDto.PhoneNumber.All(char.IsDigit))
                        {
                            ModelState.AddModelError("", $"Invalid phone number: '{simDto.PhoneNumber}'. Must be 11 digits and start with 01.");
                            await PopulateDropdownsAsync(viewModel);
                            return View(viewModel);
                        }

                        var serialNumber = string.IsNullOrWhiteSpace(simDto.SerialNumber)
                            ? $"SYS-SIM-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
                            : simDto.SerialNumber.Trim();

                        var sim = new Sim
                        {
                            SerialNumber = serialNumber,
                            PhoneNumber = simDto.PhoneNumber,
                            NetworkType = simDto.NetworkType,
                            Status = "UnAssigned",
                            IsActive = true,
                            RegisteredAt = DateTime.Now,
                            ServiceProviderId = viewModel.ServiceProviderId.Value
                        };
                        await _simRepo.AddAsync(sim);

                        var serial = new Serial
                        {
                            SerialNumber = serialNumber,
                            DocumentDetailsId = simDocumentDetails.Id,
                            SimId = sim.Id,
                            CreatedDate = DateTime.Now,
                            UserId = userId
                        };
                        await _serialRepo.AddAsync(serial);
                    }
                }

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
                        var serialNumber = string.IsNullOrWhiteSpace(usbDto.SerialNumber)
                            ? $"SYS-USB-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
                            : usbDto.SerialNumber.Trim();

                        var usb = new Usb
                        {
                            SerialNumber = serialNumber,
                            Model = usbDto.Model,
                            Status = "UnAssigned",
                            IsActive = true,
                            RegisteredAt = DateTime.Now,
                            ServiceProviderId = viewModel.ServiceProviderId.Value
                        };
                        await _usbRepo.AddAsync(usb);

                        var serial = new Serial
                        {
                            SerialNumber = serialNumber,
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