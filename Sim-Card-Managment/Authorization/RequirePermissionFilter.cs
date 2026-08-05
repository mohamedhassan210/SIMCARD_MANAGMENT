using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using System.Security.Claims;

namespace Sim_Card_Managment.Authorization
{
    public class RequirePermissionFilter : IAsyncActionFilter
    {
        private readonly string? _action;
        private readonly string? _controller;
        private readonly AppDbContext _db;

        public RequirePermissionFilter(string? action, string? controller, AppDbContext db)
        {
            _action = action;
            _controller = controller;
            _db = db;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            if (!http.User?.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var controllerName = (string.IsNullOrEmpty(_controller) ? null : _controller)
                ?? context.RouteData.Values["controller"]?.ToString();
            var actionName = (string.IsNullOrEmpty(_action) ? null : _action)
                ?? context.RouteData.Values["action"]?.ToString();

            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName))
            {
                context.Result = new ForbidResult();
                return;
            }

            var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? http.User.FindFirst("sub")?.Value
                         ?? http.User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr))
            {
                context.Result = new ForbidResult();
                return;
            }

            int? groupId = null;
            if (int.TryParse(userIdStr, out var parsedint))
            {
                groupId = await _db.Users
                    .Where(u => u.Id == parsedint && u.IsActive)
                    .Select(u => (int?)u.GroupId)
                    .FirstOrDefaultAsync();
            }
            if (groupId == null)
            {
                groupId = await _db.Users
                    .Where(u => u.Username == userIdStr && u.IsActive)
                    .Select(u => (int?)u.GroupId)
                    .FirstOrDefaultAsync();
            }
            if (groupId == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Group is inactive: sign the user out right now and send them to Login,
            // rather than just Forbid() and leaving a live session sitting around.
            var groupIsActive = await _db.Groups
                .Where(g => g.Id == groupId.Value)
                .Select(g => (bool?)g.IsActive)
                .FirstOrDefaultAsync();

            if (groupIsActive != true)
            {
                await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var tempDataFactory = http.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
                var tempData = tempDataFactory.GetTempData(http);
                tempData["Warning"] = "Your session ended because your group was deactivated. Please contact your administrator.";


                context.Result = new RedirectToActionResult("Login", "Account", new { });
                return;
            }

            var permissionId = await _db.Permissions
                .Where(p => p.ControllerName == controllerName && p.ActionName == actionName)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            if (permissionId == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var allowed = await _db.GroupPermissions
                .AnyAsync(gp => gp.GroupId == groupId.Value && gp.PermissionId == permissionId.Value);

            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}