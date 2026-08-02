// ============================================================================
// AccountControllerLoginTests.cs
// NUnit tests for AccountController.Login (GET) and AccountController.Login (POST)
// ============================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using Sim_Card_Managment.Controllers;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Services; // Ensure IEmailService namespace is imported
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerLoginTests
    {
        private Mock<IAccountRepo> _mockRepo;
        private Mock<IEmailService> _mockEmailService; // 1. Added mock for IEmailService
        private AccountController _controller;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IAccountRepo>(MockBehavior.Strict);
            _mockEmailService = new Mock<IEmailService>(); // 2. Instantiate email service mock

            _httpContext = new DefaultHttpContext();

            // 3. Pass both _mockRepo.Object and _mockEmailService.Object
            _controller = new AccountController(_mockRepo.Object, _mockEmailService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };

            _controller.TempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        // ====================================================================
        // Login (GET)
        // ====================================================================

        [Test]
        public void Login_Get_UserNotAuthenticated_ReturnsView()
        {
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // no auth type = not authenticated

            var result = _controller.Login() as ViewResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Login_Get_UserAlreadyAuthenticated_RedirectsToHomeIndex()
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "adham") }, "TestAuth");
            _httpContext.User = new ClaimsPrincipal(identity);

            var result = _controller.Login() as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        // ====================================================================
        // Login (POST)
        // ====================================================================

        [Test]
        public async Task Login_Post_InvalidModelState_ReturnsSameViewWithModel()
        {
            _controller.ModelState.AddModelError("Username", "Required");
            var model = new LoginViewmodel();

            var result = await _controller.Login(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.SameAs(model));

            // Repo should never be hit if ModelState is invalid
            _mockRepo.Verify(r => r.Login(It.IsAny<LoginViewmodel>()), Times.Never);
        }

        [Test]
        public async Task Login_Post_SuccessAndFirstLogin_SetsWarningAndRedirectsToResetPassword()
        {
            var model = new LoginViewmodel { Username = "adham", Password = "pass123" };

            _mockRepo
                .Setup(r => r.Login(model))
                .ReturnsAsync(new LoginResult { IsSuccess = true, IsFirstLogin = true });

            var result = await _controller.Login(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("ResetPassword"));
            Assert.That(result.RouteValues["username"], Is.EqualTo("adham"));
            Assert.That(_controller.TempData["Warning"], Is.EqualTo("Security Notice: You must reset your temporary password."));
        }

        [Test]
        public async Task Login_Post_SuccessNotFirstLogin_RedirectsToHomeIndex()
        {
            var model = new LoginViewmodel { Username = "adham", Password = "pass123" };

            _mockRepo
                .Setup(r => r.Login(model))
                .ReturnsAsync(new LoginResult { IsSuccess = true, IsFirstLogin = false });

            var result = await _controller.Login(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("home"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_Post_FailureWithErrorMessage_AddsModelErrorAndReturnsView()
        {
            var model = new LoginViewmodel { Username = "adham", Password = "wrongpass" };

            _mockRepo
                .Setup(r => r.Login(model))
                .ReturnsAsync(new LoginResult { IsSuccess = false, ErrorMessage = "Invalid credentials" });

            var result = await _controller.Login(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ModelState.IsValid, Is.False);
            Assert.That(_controller.ModelState[""].Errors[0].ErrorMessage, Is.EqualTo("Invalid credentials"));
        }

        [Test]
        public async Task Login_Post_FailureWithNullErrorMessage_UsesDefaultErrorMessage()
        {
            var model = new LoginViewmodel { Username = "adham", Password = "wrongpass" };

            _mockRepo
                .Setup(r => r.Login(model))
                .ReturnsAsync(new LoginResult { IsSuccess = false, ErrorMessage = null });

            var result = await _controller.Login(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ModelState[""].Errors[0].ErrorMessage, Is.EqualTo("Invalid login attempt."));
        }
    }
}