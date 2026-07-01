using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos.GroupRepos;

public class GroupRepo : IGroupRepo
{
    private readonly AppDbContext _db;
    public GroupRepo(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Group>> GetAllAsync()
    {
        var list = await _db.Groups.ToListAsync();
        return list.OrderBy(g => g.Name);
    }

    public async Task<Group?> GetByIdWithPermissionsAsync(Guid id)
        => await _db.Groups
            .Include(g => g.GroupPermissions)
                .ThenInclude(gp => gp.Permission)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task AssignPermissionsAsync(Guid groupId, IEnumerable<Guid> permissionIds)
    {
        var existing = _db.GroupPermissions.Where(gp => gp.GroupId == groupId);
        _db.GroupPermissions.RemoveRange(existing);

        foreach (var permId in permissionIds)
        {
            _db.GroupPermissions.Add(new GroupPermission
            {
                GroupId = groupId,
                PermissionId = permId
            });
        }

        await _db.SaveChangesAsync();
    }
}