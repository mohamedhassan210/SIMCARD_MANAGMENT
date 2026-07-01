using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

public class PermissionRepo : IPermissionRepo
{
    private readonly AppDbContext _db;
    public PermissionRepo(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        var list = await _db.Permissions.ToListAsync();
        return list.OrderBy(p => p.ControllerName).ThenBy(p => p.ActionName);
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
        => await _db.Permissions.FindAsync(id);
}