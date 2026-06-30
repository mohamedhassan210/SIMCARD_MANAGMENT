using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Viewmodel;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Controllers
{
    public class Accountcontroller : Controller
    {
        private readonly IAccountRepo _accountRepo;
        public Accountcontroller(IAccountRepo accountRepo)
        {
            _accountRepo = accountRepo;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var newuser = _accountRepo.Register(model);
            if (newuser)
            {
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewmodel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var Isfound = await _accountRepo.Login(model);
            if (Isfound)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var validOtpRecord = _accountRepo.GetValidOtpByEmail(model.Email);

                if (validOtpRecord != null)
                {
                    var user = _accountRepo.GetUserByEmail(model.Email);

                    if (user != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity)
                        ).Wait(); // .Wait() enforces synchronous execution here

                        TempData["Message"] = $"Your active OTP code is: {validOtpRecord.OtpCode}";
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "No active or valid OTP found for this email address.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountRepo.Logout();
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}