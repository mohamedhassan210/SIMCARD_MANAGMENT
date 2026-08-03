using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Viewmodel
{
    /// <summary>
    /// ViewModel for Permission Management Index page.
    /// Aggregates all Groups and Permissions for display.
    /// </summary>
    public class PermissionIndexViewModel
    {
        public List<Group> Groups { get; set; } = new();
        public List<Permission> Permissions { get; set; } = new();
    }
}