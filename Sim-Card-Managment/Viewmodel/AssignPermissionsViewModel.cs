using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Viewmodel
{
    /// <summary>
    /// ViewModel for Assign Permissions page.
    /// Displays a specific Group with all available Permissions grouped by Controller.
    /// </summary>
    public class AssignPermissionsViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Grouped permissions by Controller name for hierarchical display.
        /// Key = ControllerName, Value = List of permissions for that controller.
        /// </summary>
        public Dictionary<string, List<PermissionItemViewModel>> PermissionsByController { get; set; } = new();
    }

    /// <summary>
    /// Represents a single permission item with assignment state.
    /// </summary>
    public class PermissionItemViewModel
    {
        public int PermissionId { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsAssigned { get; set; }
    }
}