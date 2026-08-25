using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Adjust these to match the exact namespace of your interfaces:
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.EmployeeRepos;

namespace Sim_Card_Management.Controllers
{
    [RequirePermission]
    public class ReportController : Controller
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly ISubscriptionRepo _subscriptionRepo;

        public ReportController(IEmployeeRepo employeeRepo, ISubscriptionRepo subscriptionRepo)
        {
            _employeeRepo = employeeRepo;
            _subscriptionRepo = subscriptionRepo;

            ExcelPackage.License.SetNonCommercialPersonal("Joo");
        }

        public async Task<IActionResult> Index()
        {
            // Fetch subscriptions with related details (Sim, Usb, Employee, Quota)
            var allSubscriptions = await _subscriptionRepo.GetAllSubscriptionsWithDetailsAsync();

            // Take 2 subscriptions to display in the section preview
            var previewSubscriptions = allSubscriptions.Take(2).ToList();

            return View(previewSubscriptions);
        }

        #region --- Subscriptions Excel Report ---

        // Download Excel Report — status: "Active" | "Expired" | null/"ALL" for everything.
        // Mirrors Subscription/Index.cshtml's Status filter dropdown.
        public async Task<IActionResult> ExportSubscriptionsExcel(string? status)
        {
            byte[] fileContents = await GenerateSubscriptionsExcelBytes(status);

            var suffix = !string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase)
                ? "_" + status
                : "";

            string fileName = $"Subscriptions_Report{suffix}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // View Online (Inline Content-Disposition)
        public async Task<IActionResult> ViewSubscriptionsOnline(string? status)
        {
            byte[] fileContents = await GenerateSubscriptionsExcelBytes(status);
            Response.Headers.Add("Content-Disposition", "inline; filename=Subscriptions_Report.xlsx");
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private async Task<byte[]> GenerateSubscriptionsExcelBytes(string? status)
        {
            var subscriptions = await _subscriptionRepo.GetAllSubscriptionsWithDetailsAsync(); // Ensure your repo loads Sim, Usb, Employee, Quota

            var query = subscriptions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                bool wantActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(sub =>
                {
                    bool isActive = sub.EndDate == null || sub.EndDate > DateTime.Now;
                    return isActive == wantActive;
                });
            }

            var filteredSubscriptions = query.ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Subscriptions");

            // Define Headers — separate SIM/USB columns since a subscription can have
            // both at once, and a real Fees column (was missing entirely before).
            string[] headers = { "Subscriber", "SIM Number", "USB Serial", "Quota (GB)", "Monthly Fees", "Status", "Start Date" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(229, 80, 38)); // Brand Orange
                cell.Style.Font.Color.SetColor(Color.White);
            }

            // Fill Data
            int row = 2;
            foreach (var sub in filteredSubscriptions)
            {
                string subscriberName = sub.Employee?.Name ?? sub.NonEmployee?.Name ?? "Unassigned";
                string simNumber = sub.Sim?.PhoneNumber ?? "N/A";
                string usbSerial = sub.Usb?.SerialNumber ?? "N/A";

                decimal? quotaGb = sub.Quota != null
                    ? sub.Quota.BaseAmount + sub.Quota.ExtraAmount
                    : (decimal?)null;

                decimal fees = sub.Fees ?? 0;
                bool isActive = sub.EndDate == null || sub.EndDate > DateTime.Now;

                worksheet.Cells[row, 1].Value = subscriberName;
                worksheet.Cells[row, 2].Value = simNumber;
                worksheet.Cells[row, 3].Value = usbSerial;
                worksheet.Cells[row, 4].Value = quotaGb.HasValue ? (object)quotaGb.Value : "N/A";
                worksheet.Cells[row, 5].Value = fees;
                worksheet.Cells[row, 6].Value = isActive ? "Active" : "Expired";
                worksheet.Cells[row, 7].Value = sub.StartDate.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        #endregion

        #region --- Employees Excel Report ---

        // Download Excel Report — status: "active" | "inactive" | null/"all" for everything.
        // Mirrors Employee/Index.cshtml's Status filter dropdown.
        public async Task<IActionResult> ExportEmployeesExcel(string? status)
        {
            byte[] fileContents = await GenerateEmployeesExcelBytes(status);

            var normalizedStatus = status?.ToLower();
            var suffix = !string.IsNullOrWhiteSpace(normalizedStatus) && normalizedStatus != "all"
                ? "_" + normalizedStatus
                : "";

            string fileName = $"Employees_Report{suffix}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // View Online
        public async Task<IActionResult> ViewEmployeesOnline(string? status)
        {
            byte[] fileContents = await GenerateEmployeesExcelBytes(status);
            Response.Headers.Add("Content-Disposition", "inline; filename=Employees_Report.xlsx");
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private async Task<byte[]> GenerateEmployeesExcelBytes(string? status)
        {
            var employees = await _employeeRepo.GetPeopleListAsync("all"); // Ensure Subscriptions, Sims, and Usbs are loaded

            var normalizedStatus = status?.ToLower();
            var filteredEmployees = normalizedStatus switch
            {
                "active" => employees.Where(e => e.IsActive).ToList(),
                "inactive" => employees.Where(e => !e.IsActive).ToList(),
                _ => employees
            };

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Employees");

            // Headers
            string[] headers = { "Employee Name", "National ID / Identifier", "Status", "Total Subscriptions", "Active SIMs", "Active USBs" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(229, 80, 38));
                cell.Style.Font.Color.SetColor(Color.White);
            }

            // Fill Data
            int row = 2;
            foreach (var emp in filteredEmployees)
            {
                worksheet.Cells[row, 1].Value = emp.Name;
                worksheet.Cells[row, 2].Value = emp.Identifier;
                worksheet.Cells[row, 3].Value = emp.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 4].Value = emp.ActiveSimOnlyCount + emp.ActiveUsbCount;
                worksheet.Cells[row, 5].Value = emp.ActiveSimOnlyCount;
                worksheet.Cells[row, 6].Value = emp.ActiveUsbCount;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        #endregion
    }
}