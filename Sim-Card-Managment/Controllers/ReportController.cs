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

        // Download Excel Report
        public async Task<IActionResult> ExportSubscriptionsExcel()
        {
            byte[] fileContents = await GenerateSubscriptionsExcelBytes();
            string fileName = $"Subscriptions_Report_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // View Online (Inline Content-Disposition)
        public async Task<IActionResult> ViewSubscriptionsOnline()
        {
            byte[] fileContents = await GenerateSubscriptionsExcelBytes();
            Response.Headers.Add("Content-Disposition", "inline; filename=Subscriptions_Report.xlsx");
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private async Task<byte[]> GenerateSubscriptionsExcelBytes()
        {
            var subscriptions = await _subscriptionRepo.GetAllSubscriptionsWithDetailsAsync(); // Ensure your repo loads Sim, Usb, Employee, Quota

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Subscriptions");

            // Define Headers
            string[] headers = { "Subscriber", "Device Type", "Identifier / Serial", "Quota", "Status", "Start Date" };
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
            foreach (var sub in subscriptions)
            {
                string subscriberName = sub.Employee?.Name ?? sub.NonEmployee?.Name ?? "Unassigned";
                string deviceType = sub.Sim != null ? "SIM" : (sub.Usb != null ? "USB" : "N/A");
                string deviceNumber = sub.Sim?.PhoneNumber ?? sub.Usb?.SerialNumber ?? "N/A";
                decimal quota = sub.Quota?.BaseAmount + sub.Quota.ExtraAmount ?? 0;
                bool isActive = sub.EndDate == null || sub.EndDate > DateTime.Now;

                worksheet.Cells[row, 1].Value = subscriberName;
                worksheet.Cells[row, 2].Value = deviceType;
                worksheet.Cells[row, 3].Value = deviceNumber;
                worksheet.Cells[row, 4].Value = quota;
                worksheet.Cells[row, 5].Value = isActive ? "Active" : "Inactive";
                worksheet.Cells[row, 6].Value = sub.StartDate.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        #endregion

        #region --- Employees Excel Report ---

        // Download Excel Report
        public async Task<IActionResult> ExportEmployeesExcel()
        {
            byte[] fileContents = await GenerateEmployeesExcelBytes();
            string fileName = $"Employees_Report_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // View Online
        public async Task<IActionResult> ViewEmployeesOnline()
        {
            byte[] fileContents = await GenerateEmployeesExcelBytes();
            Response.Headers.Add("Content-Disposition", "inline; filename=Employees_Report.xlsx");
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private async Task<byte[]> GenerateEmployeesExcelBytes()
        {
            var employees = await _employeeRepo.GetPeopleListAsync("all"); // Ensure Subscriptions, Sims, and Usbs are loaded

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
            foreach (var emp in employees)
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