public class AssignPermissionsViewModel
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<PermissionCheckboxItem> Permissions { get; set; } = new();
}

public class PermissionCheckboxItem
{
    public Guid Id { get; set; }
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAssigned { get; set; }
}