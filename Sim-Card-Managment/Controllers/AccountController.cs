using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.Models;
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

            // 🔥 تم الإصلاح: الـ Repo والـ Controller بقوا متوافقين على الـ LoginResult
            var loginResult = await _accountRepo.Login(model);

            if (loginResult.IsSuccess)
            {
                if (loginResult.IsFirstLogin)
                {
                    TempData["Warning"] = "Security Notice: You must reset your temporary password.";
                    // 🛑 تم الإصلاح: غيرنا model.Email وخليناها model.Username عشان يطابق الموديل بتاعك بالظبط ويشيل الخط الأحمر
                    return RedirectToAction("ResetPassword", new { username = model.Username });
                }

                return RedirectToAction("Index", "Home");
            }

            // 🔥 تم الإصلاح: قراءة الـ ErrorMessage صح من غير أحمر
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

            var validOtpRecord = await _accountRepo.GetValidOtpByEmailAsync(model.Email);

            if (validOtpRecord != null)
            {
                var user = await _accountRepo.GetUserByEmailAsync(model.Email);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity)
                    );

                    TempData["Message"] = $"Your active OTP code is: {validOtpRecord.OtpCode}";
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "No active or valid OTP found for this email address.");
            return View(model);
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
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Registration failed.");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile(Guid id)
        {
            var userProfile = await _accountRepo.GetProfileByIdAsync(id);
            if (userProfile == null) return NotFound();

            return View(userProfile);
        }

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