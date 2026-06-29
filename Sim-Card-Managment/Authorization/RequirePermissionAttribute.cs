using Microsoft.AspNetCore.Mvc;

namespace Sim_Card_Managment.Authorization
{
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string action = null, string controller = null) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { action, controller };
        }
    }
}
