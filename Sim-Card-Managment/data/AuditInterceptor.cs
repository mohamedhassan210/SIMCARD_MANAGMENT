using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sim_Card_Managment.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Sim_Card_Managment.data
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null)
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var http = _httpContextAccessor.HttpContext;

            // No authenticated user — system operation, skip audit
            var userIdStr = http?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var ipAddress = http?.Connection?.RemoteIpAddress?.ToString();
            var module = http?.GetRouteValue("controller")?.ToString();

            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                // Never audit the audit log itself
                if (entry.Entity is AuditLog) continue;

                if (entry.State == EntityState.Unchanged ||
                    entry.State == EntityState.Detached) continue;

                var actionType = entry.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => null
                };

                if (actionType == null) continue;

                var tableName = entry.Metadata.GetTableName()
                                ?? entry.Entity.GetType().Name;

                // Resolve record Id — all your entities use int Id
                var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
                var recordId = idProp?.CurrentValue is int g ? g : 0;

                // Scalar properties only — no navigation, no circular refs
                var current = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                var original = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

                string? oldValues = null;
                string? newValues = null;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues = JsonSerializer.Serialize(current);
                        break;
                    case EntityState.Modified:
                        oldValues = JsonSerializer.Serialize(original);
                        newValues = JsonSerializer.Serialize(current);
                        break;
                    case EntityState.Deleted:
                        oldValues = JsonSerializer.Serialize(current);
                        break;
                }

                auditEntries.Add(new AuditLog
                {
                    //Id = int.Newint(),
                    TableName = tableName,
                    ActionType = actionType,
                    RecordId = recordId,
                    PerformedBy = userId,
                    PerformedAt = DateTime.UtcNow,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IPAddress = ipAddress,
                    Module = module
                });
            }

            // Same SaveChanges call — same transaction, no second round-trip
            foreach (var log in auditEntries)
                context.Set<AuditLog>().Add(log);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}