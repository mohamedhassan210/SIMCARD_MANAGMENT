using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Viewmodel;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Sim_Card_Managment.Controllers
{
    //[RequirePermission]
    public class AccountController : Controller
    {
        private readonly IAccountRepo _accountRepo;

        public AccountController(IAccountRepo accountRepo)
        {
            _accountRepo = accountRepo;
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

                return RedirectToAction("ManageUsers", "Account");
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
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential("YoussefElsayedAhmedJ5@gmail.com", "iifymjwqhvuziecx");
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("YoussefElsayedAhmedJ5@gmail.com", "SIM & USB Management System"),
                        Subject = "Your Secure Login OTP Code",
                        Body = $@"
                        <h3>Hello {user.Username},</h3>
                        <p>You requested a secure login access link via your email address.</p>
                        <p>Your active One-Time Password (OTP) code is: <strong>{validOtpRecord.OtpCode}</strong></p>
                        <p>This code is temporary. Please use it before it expires.</p>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(model.Email);
                    await smtpClient.SendMailAsync(mailMessage);
                }
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
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("YoussefElsayedAhmedJ5@gmail.com", "iifymjwqhvuziecx");
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("YoussefElsayedAhmedJ5@gmail.com", "SIM & USB Management System"),
                        Subject = "Your New Secure Login OTP Code",
                        Body = $"<h3>Your new active One-Time Password (OTP) code is: <strong>{validOtpRecord.OtpCode}</strong></h3>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);
                    await smtpClient.SendMailAsync(mailMessage);
                }

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
        // [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Register()
        {
            // جلب المجموعات من قاعدة البيانات وتحويلها لـ SelectList للـ Dropdown
            var groups = await _accountRepo.GetAllGroupsAsync();
            ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(groups, "Id", "Name");

            return View(new RegisterViewModel());
        }

        [HttpPost]
        // [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var groups = await _accountRepo.GetAllGroupsAsync();
                ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(groups, "Id", "Name");
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
            ViewBag.Groups = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(groupsRetry, "Id", "Name");
            return View(model);
        }

        #endregion

        #region 4. User Profile Details

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ProfileDetails(int id)
        {
            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int loggedInUserId))
            {
                return ForceLogoutAndRedirect();
            }

            if (loggedInUserId != id && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied");
            }

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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _accountRepo.Logout();

            TempData["Success"] = "You have been logged out securely.";
            return RedirectToAction("Login", "Account");
        }

        #endregion

        #region 6. User Management (Manager Only)

        [HttpGet]
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
        // [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditUser(int id)
        {
            var model = await _accountRepo.GetUserForEditAsync(id);
            if (model == null)
            {
                TempData["Warning"] = "The user does not exist or has been deleted.";
                return RedirectToAction("ManageUsers");
            }
            return View(model);
        }

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

        [HttpPost]
        // [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _accountRepo.SoftDeleteUserAsync(id);
            if (!result)
            {
                TempData["Warning"] = "Unable to delete user.";
            }
            else
            {
                TempData["Success"] = "User moved to soft-deleted items successfully.";
            }
            return RedirectToAction("ManageUsers");
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
    }
}