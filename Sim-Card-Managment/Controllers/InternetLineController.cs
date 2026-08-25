using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.BranchRepos;
using Sim_Card_Managment.Repos.InternetLineRepos;
using Sim_Card_Managment.Repos.LookupRepos;
using Sim_Card_Managment.Viewmodel;
using System.Drawing;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class InternetLineController : Controller
    {
        private readonly IInternetLineRepo _internetLineRepo;
        private readonly IBranchRepo _branchRepo;
        private readonly ILookupRepo _lookupRepo;
        private readonly IServiceProviderRepository _serviceProviderRepo;
        private readonly ISIMRepo _simRepo;

        public InternetLineController(
            IInternetLineRepo internetLineRepo,
            IBranchRepo branchRepo,
            ILookupRepo lookupRepo,
            IServiceProviderRepository serviceProviderRepo,
            ISIMRepo simRepo)
        {
            _internetLineRepo = internetLineRepo;
            _branchRepo = branchRepo;
            _lookupRepo = lookupRepo;
            _serviceProviderRepo = serviceProviderRepo;
            _simRepo = simRepo;
        }

        private async Task PopulateDropdowns(InternetLineCreateViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var paymentTypes = await _lookupRepo.GetPaymentTypesAsync();
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();
            var renewalTypes = await _lookupRepo.GetRenewalTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.PaymentTypes = paymentTypes
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ServiceTypes = serviceTypes
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            model.RenewalTypes = renewalTypes
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
            model.RenewalTypeDurations = renewalTypes
                .ToDictionary(r => r.Id, r => r.DurationInMonths);
            model.ServiceTypeHasPhoneNumber = serviceTypes
                .ToDictionary(s => s.Id, s => s.HasPhoneNumber);
        }

        private async Task PopulateDropdowns(InternetLineEditViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var paymentTypes = await _lookupRepo.GetPaymentTypesAsync();
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();
            var renewalTypes = await _lookupRepo.GetRenewalTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.PaymentTypes = paymentTypes
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ServiceTypes = serviceTypes
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
            model.RenewalTypes = renewalTypes
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
            model.RenewalTypeDurations = renewalTypes
                .ToDictionary(r => r.Id, r => r.DurationInMonths);
            model.ServiceTypeHasPhoneNumber = serviceTypes
                .ToDictionary(s => s.Id, s => s.HasPhoneNumber);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lines = await _internetLineRepo.GetAllAsync();
            return View(lines);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var line = await _internetLineRepo.GetByIdWithDetailsAsync(id);
            if (line == null) return NotFound();
            return View(line);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new InternetLineCreateViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InternetLineCreateViewModel model)
        {
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();
            var selectedType = serviceTypes.FirstOrDefault(s => s.Id == model.ServiceTypeId);
            bool isLandline = selectedType?.HasPhoneNumber == true;

            if (isLandline)
            {
                model.SimId = null;
                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                    ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is required for this service type.");
            }
            else
            {
                if (model.SimId == null)
                    ModelState.AddModelError(nameof(model.SimId), "Please select a SIM card for this service type.");
            }

            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
            {
                model.CreatedById = currentUserId;
            }
            else
            {
                model.CreatedById = 1; // Default fallback ID if unauthenticated in dev
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            await _internetLineRepo.AddAsync(model);

            TempData["Success"] = "Internet line created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _internetLineRepo.GetForEditAsync(id);
            if (model == null) return NotFound();
            await PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InternetLineEditViewModel model)
        {
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();
            var selectedType = serviceTypes.FirstOrDefault(s => s.Id == model.ServiceTypeId);
            bool isLandline = selectedType?.HasPhoneNumber == true;

            if (isLandline)
            {
                model.SimId = null;
                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                    ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is required for this service type.");
            }
            else
            {
                if (model.SimId == null)
                    ModelState.AddModelError(nameof(model.SimId), "Please select a SIM card for this service type.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            await _internetLineRepo.UpdateAsync(model);
            TempData["Success"] = "Internet line updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id, string? returnAction = nameof(Index))
        {
            var renewed = await _internetLineRepo.RenewAsync(id);

            TempData[renewed ? "Success" : "Error"] = renewed
                ? "Internet line renewed successfully."
                : "Internet line was not found or has no renewal type set.";

            return RedirectToAction(returnAction);
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var lines = await _internetLineRepo.GetForDashboardAsync();
            return View(lines);
        }
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> SearchSims(string query, int? currentLineId = null)
        {
            var sims = await _simRepo.GetAssignableSimsForInternetLineAsync(query, currentLineId);

            var result = sims.Select(s =>
            {
                var activeSub = s.Subscriptions?
                    .FirstOrDefault(sub => sub.EndDate == null || sub.EndDate > DateTime.Now);

                string? assignedTo = activeSub?.Employee != null
                    ? activeSub.Employee.Name
                    : activeSub?.NonEmployee != null
                        ? activeSub.NonEmployee.Name
                        : null;

                return new
                {
                    id = s.Id,
                    phoneNumber = s.PhoneNumber,
                    serialNumber = s.SerialNumber,
                    providerId = s.ServiceProviderId,
                    providerName = s.ServiceProvider?.Name ?? "Unknown",
                    assignedTo
                };
            });

            return Json(result);
        }

        #region Reports

        [HttpGet]
        public async Task<IActionResult> Report(string? searchTerm)
        {
            var branches = await _branchRepo.GetAllAsync();
            var allInternetLines = await _internetLineRepo.GetAllAsync();

            var model = new InternetLineReportViewModel();

            foreach (var branch in branches)
            {
                var internetLines = allInternetLines
                    .Where(il => il.BranchName == branch.Name).ToList();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    internetLines = internetLines.Where(il =>
                        il.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (il.ServiceProviderName != null && il.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (il.PhoneNumber != null && il.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                model.Branches.Add(new BranchInternetLineItem
                {
                    BranchName = branch.Name,
                    IsActive = branch.IsActive,
                    InternetLines = internetLines
                });
            }

            return View(model);
        }

        
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? searchTerm, string status = "all")
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            var data = await _internetLineRepo.GetForExcelAsync();
            status = (status ?? "all").ToLower();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                data = data
                    .Where(b =>
                        b.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.InternetLines.Any(il =>
                            il.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            il.PhoneNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                            il.ServiceTypeName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .ToList();
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Internet");

            // =========================================================
            // TITLE
            // =========================================================
            const int totalColumns = 10; // Branch, ISP, Payment, Service Type, SIM SN, Phone, Renewal Date, Quota, Bandwidth, Status, Notes
                                         // (11 actually — see header list below; keep this const in sync if columns change)

            string[] headers = {
        "Branch", "ISP", "Payment", "Service Type", "SIM SN",
        "Phone Number", "Renewal Date", "Quota", "Bandwidth", "Status", "Notes"
    };
            int lastColumn = headers.Length;

            worksheet.Cells[1, 1, 1, lastColumn].Merge = true;
            worksheet.Cells[1, 1].Value = "Internet Service";
            var titleRange = worksheet.Cells[1, 1, 1, lastColumn];
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 16;
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // =========================================================
            // HEADER — fixed columns, no per-service-type expansion
            // =========================================================
            int headerRow = 2;
            for (int i = 0; i < headers.Length; i++)
                worksheet.Cells[headerRow, i + 1].Value = headers[i];

            using (var headerRange = worksheet.Cells[headerRow, 1, headerRow, lastColumn])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // =========================================================
            // DATA — one row per internet line, branch repeated per row
            // =========================================================
            int row = 3;
            foreach (var branch in data.OrderBy(x => x.BranchName))
            {
                var lines = branch.InternetLines.AsEnumerable();
                if (status == "up") lines = lines.Where(l => l.Status);
                else if (status == "down") lines = lines.Where(l => !l.Status);
                var filteredLines = lines.ToList();

                if (!filteredLines.Any())
                    continue;

                foreach (var line in filteredLines)
                {
                    worksheet.Cells[row, 1].Value = branch.BranchName;
                    worksheet.Cells[row, 2].Value = line.ServiceProviderName;
                    worksheet.Cells[row, 3].Value = line.PaymentTypeName;
                    worksheet.Cells[row, 4].Value = line.ServiceTypeName;
                    worksheet.Cells[row, 5].Value = line.SimSerialNumber ?? "N/A";
                    worksheet.Cells[row, 6].Value = line.PhoneNumber ?? "N/A";
                    worksheet.Cells[row, 7].Value = line.NextRenewalDate.HasValue
                        ? line.NextRenewalDate.Value.ToString("dd MMM yyyy")
                        : "N/A";
                    worksheet.Cells[row, 8].Value = line.QuotaGB.HasValue ? $"{line.QuotaGB} GB" : "N/A";
                    worksheet.Cells[row, 9].Value = line.Bandwidth ?? "N/A";
                    worksheet.Cells[row, 10].Value = line.Status ? "UP" : "DOWN";
                    worksheet.Cells[row, 11].Value = line.Notes ?? "";

                    row++;
                }
            }

            // =========================================================
            // GENERAL STYLING
            // =========================================================
            if (row > 3)
            {
                using var dataRange = worksheet.Cells[3, 1, row - 1, lastColumn];
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                dataRange.Style.WrapText = true;

                // Status column color coding
                var statusColumn = 10;
                for (int r = 3; r < row; r++)
                {
                    var statusCell = worksheet.Cells[r, statusColumn];
                    statusCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var cellStatus = statusCell.Value?.ToString() ?? "";   // <-- new local, not `status`
                    if (cellStatus == "UP")
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                    }
                    else if (cellStatus == "DOWN")
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }
                }
            }

            worksheet.Column(1).Width = 18;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            for (int c = 2; c <= lastColumn; c++)
            {
                if (worksheet.Column(c).Width > 30)
                    worksheet.Column(c).Width = 30;
            }
            worksheet.Row(1).Height = 30;
            worksheet.View.FreezePanes(3, 2);

            var fileContents = package.GetAsByteArray();
            var suffix = status != "all" ? "_" + (status == "up" ? "Up" : "Down") : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Internet_Lines{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        #endregion

        private static void SetInternetCell(
    ExcelWorksheet worksheet,
    int row,
    List<string> columns,
    string columnName,
    IEnumerable<string?> values)
        {
            int column = columns.IndexOf(columnName) + 1;

            if (column <= 0)
                return;

            var cleanedValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            worksheet.Cells[row, column].Value =
                string.Join(
                    Environment.NewLine,
                    cleanedValues);
        }
    }
}