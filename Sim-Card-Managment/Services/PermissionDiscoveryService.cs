using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Sim_Card_Managment.Authorization;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Services
{
    public class PermissionDiscoveryService
    {
        private readonly IActionDescriptorCollectionProvider _provider;

        public PermissionDiscoveryService(IActionDescriptorCollectionProvider provider)
        {
            _provider = provider;
        }

        public async Task SeedPermissionsAsync(AppDbContext db)
        {
            var discovered = _provider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Select(d =>
                {
                    var attr =
                        d.MethodInfo.GetCustomAttributes(typeof(RequirePermissionAttribute), true)
                            .FirstOrDefault() as RequirePermissionAttribute
                        ?? d.ControllerTypeInfo.GetCustomAttributes(typeof(RequirePermissionAttribute), true)
                            .FirstOrDefault() as RequirePermissionAttribute;

                    if (attr == null) return null;

                    return new
                    {
                        Controller = attr.Controller ?? d.ControllerName,
                        Action = attr.Action ?? d.ActionName
                    };
                })
                .Where(x => x != null)
                .DistinctBy(x => (x!.Controller, x.Action))
                .ToList();

            // Load existing keys in one query to avoid N+1 inside the loop
            var existing = db.Permissions
                .Select(p => p.ControllerName + "|" + p.ActionName)
                .ToHashSet();

            foreach (var item in discovered)
            {
                if (existing.Contains(item!.Controller + "|" + item.Action))
                    continue;

                db.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    ControllerName = item.Controller,
                    ActionName = item.Action,
                    Description = $"{item.Controller}.{item.Action}"
                });
            }

            await db.SaveChangesAsync();
        }
    }
}