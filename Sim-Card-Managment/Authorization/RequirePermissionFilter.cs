using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sim_Card_Managment.data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

            // Ensure authenticated
            if (!http.User?.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Determine controller/action names
            var controllerName = _controller ?? context.RouteData.Values["controller"]?.ToString();
            var actionName = _action ?? context.RouteData.Values["action"]?.ToString();

            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Get user id from claims
            string? userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? http.User.FindFirst("sub")?.Value
                                ?? http.User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Load users into memory and attempt to resolve the current user and its GroupId
            var users = await _db.Users.ToListAsync();
            object? matchedUser = null;

            // Try parse guid
            Guid parsedGuid;
            bool isGuid = Guid.TryParse(userIdStr, out parsedGuid);

            foreach (var u in users)
            {
                var type = u.GetType();

                if (isGuid)
                {
                    // check common id property names
                    var idProp = type.GetProperty("Id") ?? type.GetProperty("UserId") ?? type.GetProperty("ID");
                    if (idProp != null)
                    {
                        var val = idProp.GetValue(u);
                        if (val is Guid g && g == parsedGuid)
                        {
                            matchedUser = u;
                            break;
                        }
                    }
                }

                // fallback: check username/name matching
                var nameProp = type.GetProperty("Username") ?? type.GetProperty("UserName") ?? type.GetProperty("Name");
                if (nameProp != null)
                {
                    var nv = nameProp.GetValue(u)?.ToString();
                    if (!string.IsNullOrEmpty(nv) && nv == userIdStr)
                    {
                        matchedUser = u;
                        break;
                    }
                }
            }

            if (matchedUser == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // get GroupId from matched user via common property names
            var matchedType = matchedUser.GetType();
            var groupProp = matchedType.GetProperty("GroupId") ?? matchedType.GetProperty("Group") ?? matchedType.GetProperty("RoleId");
            Guid groupId;
            if (groupProp == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var groupVal = groupProp.GetValue(matchedUser);
            if (groupVal == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (groupVal is Guid gId)
            {
                groupId = gId;
            }
            else if (Guid.TryParse(groupVal.ToString(), out var tmp))
            {
                groupId = tmp;
            }
            else
            {
                context.Result = new ForbidResult();
                return;
            }

            // find permission
            var permission = _db.Permissions.FirstOrDefault(p => p.ControllerName == controllerName && p.ActionName == actionName);
            if (permission == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var allowed = _db.GroupPermissions.Any(gp => gp.GroupId == groupId && gp.PermissionId == permission.Id);
            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
