using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Viewmodel;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Controllers
{
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
                return RedirectToAction("Index", "Home");
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

                return RedirectToAction("Index", "Home");
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
                // Security best practice: don't reveal if an email doesn't exist, 
                // or show a generic error to prevent email harvesting.
                ModelState.AddModelError("", "This email address is not registered in the system.");
                return View(model);
            }

            // Try to find an existing valid OTP first
            var validOtpRecord = await _accountRepo.GetValidOtpByEmailAsync(model.Email);

            // If no valid OTP exists, generate a brand new one on the fly!
            if (validOtpRecord == null)
            {
                // Example: Generate a random 6-digit number
                string newOtpCode = new Random().Next(100000, 999999).ToString();

                // Save it via your repository layer
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
            catch (Exception ex)
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
            // Retrieve the target email passed from the forgot password panel step
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

            // Fetch the latest OTP record for this email
            var validOtpRecord = await _accountRepo.GetValidOtpByEmailAsync(model.Email);

            if (validOtpRecord == null)
            {
                ModelState.AddModelError("", "No verification code was requested for this email address.");
                return View(model);
            }

            // 1. Check Expiration first
            if (validOtpRecord.ExpireDate < DateTime.Now)
            {
                ModelState.AddModelError("", "This OTP has expired. Please request a new code.");
                return View(model);
            }

            // 2. Check if already used (if your DB tracks this)
            if (validOtpRecord.IsUsed)
            {
                ModelState.AddModelError("", "This OTP has already been used. Please request a new code.");
                return View(model);
            }

            // 3. Check if the code matches
            if (validOtpRecord.OtpCode != model.OtpCode.Trim())
            {
                ModelState.AddModelError("", "Incorrect OTP. Please check the code and try again.");
                return View(model);
            }

            // --- Success Flow ---
            var user = await _accountRepo.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                // Clear session limit tracking on successful login
                HttpContext.Session.Remove("ResendCount_" + model.Email);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                TempData["Success"] = "Logged in successfully.";
                return RedirectToAction("Index", "Home");
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

            // 🛑 Rule Check: If they hit 10 resends, cut them off
            if (currentResends >= 10)
            {
                TempData["ErrorMessage"] = "You have reached the maximum layout of 10 resends. Please contact support at support@company.com.";
                TempData["TargetEmail"] = email;
                return RedirectToAction("VerifyOtp");
            }

            // Increment resend count tracker
            currentResends++;
            HttpContext.Session.SetInt32(sessionKey, currentResends);

            // Generate a new OTP code
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

        #region 3. User Registration & Profile Management

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var isCreated = _accountRepo.Register(model);
            if (isCreated)
            {
                TempData["Success"] = "Account created successfully.";
                return RedirectToAction("ManageUsers");
            }

            ModelState.AddModelError("", "Registration failed.");
            return View(model);
        }

        #endregion

        #region 4. User Profile Details (Advanced Security)

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ProfileDetails(Guid id)
        {
            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out Guid loggedInUserId))
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

        #region 🔥 6. User Management (Advanced Features - Manager Only) 🔥

        // 1. شاشة عرض وإدارة المستخدمين مع البحث والفلترة
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManageUsers(string? search, Guid? groupId, bool? isActive)
        {
            var users = await _accountRepo.GetAllUsersAsync(search, groupId, isActive);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentGroupId = groupId;
            ViewBag.CurrentIsActive = isActive;

            return View(users);
        }

        // 2. شاشة تعديل بيانات مستخدم (HttpGet)
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var model = await _accountRepo.GetUserForEditAsync(id);
            if (model == null)
            {
                TempData["Warning"] = "The user does not exist or has been deleted.";
                return RedirectToAction("ManageUsers");
            }
            return View(model);
        }

        // 3. استقبال بيانات التعديل وحفظها (HttpPost)
        [HttpPost]
        [Authorize(Roles = "Manager")]
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

        // 4. تجميد أو تفعيل الحساب بزرار واحد سريع (Toggle Active)
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
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

        // 5. الحذف الذكي بدون مسح نهائي (Soft Delete)
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(Guid id)
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