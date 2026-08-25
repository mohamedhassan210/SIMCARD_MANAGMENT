using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Services;
using Sim_Card_Managment.Viewmodel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Controllers
{
    //[RequirePermission]
    public class AccountController : Controller
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IEmailService _emailService;


        public AccountController(IAccountRepo accountRepo, IEmailService emailService)
        {
            _accountRepo = accountRepo;
            _emailService = emailService;
        }

        #region 1. Authentication (Login & Force Password Reset)

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("home", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewmodel model)
        {
            if (!ModelState.IsValid) return View(model);

            var loginResult = await _accountRepo.Login(model);

            if (loginResult.IsSuccess)
            {
                if (loginResult.IsFirstLogin)
                {
                    TempData["Warning"] = "Security Notice: You must reset your temporary password.";
                    return RedirectToAction("ResetPassword", new { username = model.Username });
                }

                return RedirectToAction("home", "Home");
            }

            ModelState.AddModelError("", loginResult.ErrorMessage ?? "Invalid login attempt.");
            return View(model);
        }

        #endregion

        #region 2. Password Management (Reset & Forgot Password)

        [HttpGet]
        public IActionResult ResetPassword(string username)
        {
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login");

            var model = new ResetPasswordViewModel { Username = username };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _accountRepo.ResetPasswordAsync(model);
            if (result)
            {
                TempData["Success"] = "Password updated successfully. Please log in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Error resetting password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _accountRepo.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "This email address is not registered in the system.");
                return View(model);
            }

            var validOtpRecord = await _accountRepo.GetValidOtpByEmailAsync(model.Email);

            if (validOtpRecord == null)
            {
                string newOtpCode = new Random().Next(100000, 999999).ToString();
                validOtpRecord = await _accountRepo.CreateAndSaveNewOtpAsync(model.Email, newOtpCode);
            }

            try
            {
                await _emailService.SendEmailAsync(
                    model.Email,
                    "Your Secure Login OTP Code",
                    $@"<h3>Hello {user.Username},</h3>
           <p>You requested a secure login access link via your email address.</p>
           <p>Your active One-Time Password (OTP) code is: <strong>{validOtpRecord.OtpCode}</strong></p>
           <p>This code is temporary. Please use it before it expires.</p>");
            }
            catch
            {
                ModelState.AddModelError("", "Failed to send the email. Please contact your system administrator.");
                return View(model);
            }

            TempData["TargetEmail"] = model.Email;
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = TempData["TargetEmail"] as string;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            var model = new VerifyOtpViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var validOtpRecord = await _accountRepo.GetValidOtpByEmailAsync(model.Email);

            if (validOtpRecord == null)
            {
                ModelState.AddModelError("", "No verification code was requested for this email address.");
                return View(model);
            }

            if (validOtpRecord.ExpireDate < DateTime.Now)
            {
                ModelState.AddModelError("", "This OTP has expired. Please request a new code.");
                return View(model);
            }

            if (validOtpRecord.IsUsed)
            {
                ModelState.AddModelError("", "This OTP has already been used. Please request a new code.");
                return View(model);
            }

            if (validOtpRecord.OtpCode != model.OtpCode.Trim())
            {
                ModelState.AddModelError("", "Incorrect OTP. Please check the code and try again.");
                return View(model);
            }

            var user = await _accountRepo.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                HttpContext.Session.Remove("ResendCount_" + model.Email);
                return RedirectToAction("ResetPassword", new { username = user.Username });
            }

            ModelState.AddModelError("", "An error occurred. Please try again.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            string sessionKey = $"ResendCount_{email}";
            int currentResends = HttpContext.Session.GetInt32(sessionKey) ?? 0;

            if (currentResends >= 10)
            {
                TempData["ErrorMessage"] = "You have reached the maximum layout of 10 resends. Please contact support.";
                TempData["TargetEmail"] = email;
                return RedirectToAction("VerifyOtp");
            }

            currentResends++;
            HttpContext.Session.SetInt32(sessionKey, currentResends);

            string newOtpCode = new Random().Next(100000, 999999).ToString();
            var validOtpRecord = await _accountRepo.CreateAndSaveNewOtpAsync(email, newOtpCode);

            try
            {
                await _emailService.SendEmailAsync(
                    email,
                    "Your New Secure Login OTP Code",
                    $"<h3>Your new active One-Time Password (OTP) code is: <strong>{validOtpRecord.OtpCode}</strong></h3>");

                TempData["SuccessMessage"] = $"A new code has been sent! (Resend request {currentResends}/10)";
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to dispatch email server routing. Please try again.";
            }

            TempData["TargetEmail"] = email;
            return RedirectToAction("VerifyOtp");
        }

        #endregion

        #region 3. User Registration (Manager-Only)

        [HttpGet]
        [RequirePermission]
        public async Task<IActionResult> Register()
        {
            var groups = await _accountRepo.GetAllGroupsAsync();
            var activeGroups = groups.Where(g => g.IsActive).ToList();
            ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(activeGroups, "Id", "Name");
            return View(new RegisterViewModel());
        }

        [RequirePermission]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var groups = await _accountRepo.GetAllGroupsAsync();
                ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(groups.Where(g => g.IsActive), "Id", "Name");
                return View(model);
            }

            // ← add email check
            var existingUser = await _accountRepo.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                var groups = await _accountRepo.GetAllGroupsAsync();
                ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(groups.Where(g => g.IsActive), "Id", "Name");
                return View(model);
            }

            var isCreated = _accountRepo.Register(model);
            if (isCreated)
            {
                TempData["Success"] = "Account created successfully.";
                return RedirectToAction("ManageUsers");
            }

            ModelState.AddModelError("", "Registration failed.");
            var groupsRetry = await _accountRepo.GetAllGroupsAsync();
            ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(groupsRetry.Where(g => g.IsActive), "Id", "Name");
            return View(model);
        }

        #endregion

        #region 4. User Profile Details
        [RequirePermission]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userProfile = await _accountRepo.GetProfileByIdAsync(id);
            if (userProfile == null) return NotFound();

            return View(userProfile);
        }

        #endregion

        #region 5. Secure Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // 1. Sign out cookie scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Perform backend cleanup (clear tokens, log activity, etc.)
            await _accountRepo.Logout();

            // 3. Provide feedback and redirect
            TempData["Success"] = "You have been logged out securely.";
            return RedirectToAction("Login", "Account");
        }

        #endregion

        #region 6. User Management (Manager Only)

        [HttpGet]
        [RequirePermission]
        // [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManageUsers(string? search, int? groupId, bool? isActive)
        {
            var users = await _accountRepo.GetAllUsersAsync(search, groupId, isActive);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentGroupId = groupId;
            ViewBag.CurrentIsActive = isActive;

            return View(users);
        }

        [HttpGet]
        [RequirePermission]
        public async Task<IActionResult> ExportUsersExcel(bool? isActive)
        {
            var users = await _accountRepo.GetAllUsersAsync(null, null, isActive);

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Users");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "Group";
            worksheet.Cells[1, 4].Value = "Status";

            using (var headerRange = worksheet.Cells[1, 1, 1, 4])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cells[row, 1].Value = user.Username;
                worksheet.Cells[row, 2].Value = user.Email;
                worksheet.Cells[row, 3].Value = user.GroupName;
                worksheet.Cells[row, 4].Value = user.IsActive ? "Active" : "Inactive";
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();

            var suffix = isActive == true ? "_Active" : isActive == false ? "_Inactive" : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Users{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        [HttpGet]
        [RequirePermission]
        public async Task<IActionResult> EditUser(int id)
        {
            var model = await _accountRepo.GetUserForEditAsync(id);
            if (model == null)
            {
                TempData["Warning"] = "The user does not exist or has been deleted.";
                return RedirectToAction("ManageUsers");
            }

            var groups = await _accountRepo.GetAllGroupsAsync();
            ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(groups, "Id", "Name", model.GroupId);

            return View(model);
        }

        [RequirePermission]

        [HttpPost]
        // [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _accountRepo.UpdateUserAsync(model);
            if (result)
            {
                TempData["Success"] = "User data updated successfully.";
                return RedirectToAction("ManageUsers");
            }

            TempData["Warning"] = "An error occurred while updating user data.";
            return View(model);
        }

        [RequirePermission]
        [HttpPost]
        // [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _accountRepo.ToggleUserActiveAsync(id);
            if (!result)
            {
                TempData["Warning"] = "Unable to change account status.";
            }
            else
            {
                TempData["Success"] = "Account status updated successfully.";
            }
            return RedirectToAction("ManageUsers");
        }

        [RequirePermission]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _accountRepo.SoftDeleteUserAsync(id);
            if (!result)
            {
                TempData["Warning"] = "Unable to delete user.";
                return RedirectToAction("ManageUsers");
            }

            TempData["Success"] = "User moved to soft-deleted items successfully.";

            // Check if the deleted user is the currently logged-in user
            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdClaim, out int loggedInUserId) && loggedInUserId == id)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("ManageUsers");
        }

        [RequirePermission]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(int id)
        {
            await _accountRepo.ActivateUserAsync(id);
            TempData["Success"] = "User activated successfully.";
            return RedirectToAction("Details", new { id });
        }

        #endregion

        #region 7. Helpers

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private RedirectToActionResult ForceLogoutAndRedirect()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        #endregion
        #region 8. Change Password (Logged-in User)

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return ForceLogoutAndRedirect();
            }

            var result = await _accountRepo.ChangePasswordAsync(currentUserId, model.NewPassword);

            if (!result.IsSuccess)
            {
                return View(model);
            }

            TempData["Success"] = "Your password has been updated successfully.";
            return RedirectToAction("Login");
        }

        #endregion
    }
}