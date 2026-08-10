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

        public InternetLineController(
            IInternetLineRepo internetLineRepo,
            IBranchRepo branchRepo,
            ILookupRepo lookupRepo,
            IServiceProviderRepository serviceProviderRepo)
        {
            _internetLineRepo = internetLineRepo;
            _branchRepo = branchRepo;
            _lookupRepo = lookupRepo;
            _serviceProviderRepo = serviceProviderRepo;
        }

        private async Task PopulateDropdowns(InternetLineCreateViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var paymentTypes = await _lookupRepo.GetPaymentTypesAsync();
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.PaymentTypes = paymentTypes
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ServiceTypes = serviceTypes
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
        }

        private async Task PopulateDropdowns(InternetLineEditViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var paymentTypes = await _lookupRepo.GetPaymentTypesAsync();
            var serviceTypes = await _lookupRepo.GetServiceTypesAsync();

            model.Branches = branches.Where(b => b.IsActive)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
            model.ServiceProviders = providers.Where(p => p.IsActive)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.PaymentTypes = paymentTypes
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });
            model.ServiceTypes = serviceTypes
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
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
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            await _internetLineRepo.UpdateAsync(model);
            TempData["Success"] = "Internet line updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
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
        public async Task<IActionResult> ExportToExcel(string? searchTerm)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            // Get all Internet Line data
            var data = await _internetLineRepo.GetForExcelAsync();

            // Get ALL service types dynamically from DB
            var serviceTypes = (await _lookupRepo.GetServiceTypesAsync())
                .OrderBy(x => x.Id)
                .ToList();

            // Apply search if needed
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

            worksheet.Cells[1, 1].Value = "Internet Service";

            // Number of columns:
            // Branch + (8 columns per Service Type) + Notes
            int totalColumns = 1 + (serviceTypes.Count * 8) + 1;

            worksheet.Cells[1, 1, 1, totalColumns].Merge = true;

            var titleRange = worksheet.Cells[1, 1, 1, totalColumns];

            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 16;
            titleRange.Style.HorizontalAlignment =
                OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment =
                OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            titleRange.Style.Fill.BackgroundColor.SetColor(Color.White);

            // =========================================================
            // HEADER
            // =========================================================

            int headerRow = 2;
            int column = 1;

            // Branch
            worksheet.Cells[headerRow, column].Value = "Branch";
            column++;

            // Dynamic ServiceType columns
            foreach (var serviceType in serviceTypes)
            {
                string typeName = serviceType.Name;

                worksheet.Cells[headerRow, column].Value = $"{typeName} ISP";
                column++;

                worksheet.Cells[headerRow, column].Value = $"{typeName} Payment";
                column++;

                worksheet.Cells[headerRow, column].Value = $"{typeName} Phone Number";
                column++;

                worksheet.Cells[headerRow, column].Value = $"{typeName} SIM SN";
                column++;

                worksheet.Cells[headerRow, column].Value = $"{typeName} Renewal Date";
                column++;

                worksheet.Cells[headerRow, column].Value = $"{typeName} Quota";
                column++;

                worksheet.Cells[headerRow, column].Value = $"{typeName} Bandwidth";
                column++;

                worksheet.Cells[headerRow, column].Value = $"{typeName} Status";
                column++;
            }

            // Notes
            worksheet.Cells[headerRow, column].Value = "Notes";

            int lastColumn = column;

            // =========================================================
            // HEADER STYLE
            // =========================================================

            using (var headerRange = worksheet.Cells[headerRow, 1, headerRow, lastColumn])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment =
                    OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // =========================================================
            // DATA
            // =========================================================

            int row = 3;

            foreach (var branch in data.OrderBy(x => x.BranchName))
            {
                worksheet.Cells[row, 1].Value = branch.BranchName;

                column = 2;

                foreach (var serviceType in serviceTypes)
                {
                    string typeName = serviceType.Name;

                    // All lines belonging to this Service Type
                    var lines = branch.InternetLines
                        .Where(x => x.ServiceTypeName.Equals(
                            typeName,
                            StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // -------------------------------------------------
                    // Multiple records go into SAME CELL using newline
                    // -------------------------------------------------

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.ServiceProviderName)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                        );

                    column++;

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.PaymentTypeName)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                        );

                    column++;

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.PhoneNumber)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                        );

                    column++;

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.SimSerialNumber)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                        );

                    column++;

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.RenewalDay.HasValue
                                    ? $"{x.RenewalDay}th every month"
                                    : null)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                        );

                    column++;

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.QuotaGB.HasValue
                                    ? $"{x.QuotaGB} GB"
                                    : null)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                        );

                    column++;

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.Bandwidth)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                        );

                    column++;

                    worksheet.Cells[row, column].Value =
                        string.Join(
                            Environment.NewLine,
                            lines
                                .Select(x => x.Status ? "UP" : "DOWN")
                                .Distinct()
                        );

                    column++;
                }

                // Notes
                worksheet.Cells[row, column].Value =
                    string.Join(
                        Environment.NewLine,
                        branch.InternetLines
                            .Select(x => x.Notes)
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct()
                    );

                row++;
            }

            // =========================================================
            // GENERAL STYLING
            // =========================================================

            if (row > 3)
            {
                using var dataRange =
                    worksheet.Cells[3, 1, row - 1, lastColumn];

                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                dataRange.Style.VerticalAlignment =
                    ExcelVerticalAlignment.Center;

                dataRange.Style.WrapText = true;
            }

            // Green status cells
            for (int r = 3; r < row; r++)
            {
                column = 2;

                foreach (var serviceType in serviceTypes)
                {
                    // Skip:
                    // ISP
                    // Payment
                    // Phone
                    // SIM
                    // Renewal
                    // Quota
                    // Bandwidth

                    column += 7;

                    var statusCell = worksheet.Cells[r, column];

                    statusCell.Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;

                    if (statusCell.Value != null)
                    {
                        string status = statusCell.Value.ToString() ?? "";

                        if (status.Contains("UP"))
                        {
                            statusCell.Style.Fill.PatternType =
                                ExcelFillStyle.Solid;

                            statusCell.Style.Fill.BackgroundColor
                                .SetColor(Color.LimeGreen);
                        }
                        else if (status.Contains("DOWN"))
                        {
                            statusCell.Style.Fill.PatternType =
                                ExcelFillStyle.Solid;

                            statusCell.Style.Fill.BackgroundColor
                                .SetColor(Color.LightCoral);
                        }
                    }

                    column++;
                }
            }

            // =========================================================
            // BRANCH COLUMN
            // =========================================================

            worksheet.Column(1).Width = 18;

            // =========================================================
            // AUTO SIZE
            // =========================================================

            worksheet.Cells[worksheet.Dimension.Address]
                .AutoFitColumns();

            // Keep columns from becoming ridiculously wide
            for (int c = 2; c <= lastColumn; c++)
            {
                if (worksheet.Column(c).Width > 30)
                    worksheet.Column(c).Width = 30;
            }

            worksheet.Row(1).Height = 30;

            // Freeze headers
            worksheet.View.FreezePanes(3, 2);

            // =========================================================
            // RETURN FILE
            // =========================================================

            var fileContents = package.GetAsByteArray();

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Internet_Lines_{DateTime.Now:yyyyMMdd}.xlsx"
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