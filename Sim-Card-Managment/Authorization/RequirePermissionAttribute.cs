using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Authorization;

public class RequirePermissionAttribute : TypeFilterAttribute
{
    public string? Action { get; }
    public string? Controller { get; }

    public RequirePermissionAttribute(string? action = null, string? controller = null)
        : base(typeof(RequirePermissionFilter))
    {
        Action = action;
        Controller = controller;
        Arguments = new object[] { action, controller };
    }
}