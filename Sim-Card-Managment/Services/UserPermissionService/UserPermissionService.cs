using Sim_Card_Managment.Repos.GroupRepos;

namespace Sim_Card_Managment.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGroupRepo _groupRepo;

        public UserPermissionService(IHttpContextAccessor httpContextAccessor, IGroupRepo groupRepo)
        {
            _httpContextAccessor = httpContextAccessor;
            _groupRepo = groupRepo;
        }

        public async Task<HashSet<string>> GetPermissionKeysAsync()
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return keys;

            var groupIdStr = user.FindFirst("GroupId")?.Value;
            if (!int.TryParse(groupIdStr, out var groupId))
                return keys;

            var group = await _groupRepo.GetByIdWithPermissionsAsync(groupId);
            if (group == null) return keys;

            foreach (var gp in group.GroupPermissions)
            {
                if (gp.Permission != null)
                    keys.Add($"{gp.Permission.ControllerName}|{gp.Permission.ActionName}");
            }

            return keys;
        }

        public async Task<bool> HasPermissionAsync(string controller, string action)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null && user.IsInRole("Manager"))
                return true; // Managers see everything

            var keys = await GetPermissionKeysAsync();
            return keys.Contains($"{controller}|{action}");
        }
    }
}