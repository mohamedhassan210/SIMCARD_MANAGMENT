using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.GroupRepos
{
    public class GroupRepo : IGroupRepo
    {
        private readonly AppDbContext _context;

        public GroupRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _context.Groups
                .Include(g => g.CreatedBy)
                .Include(g => g.Users)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Group?> GetByIdAsync(int id)
        {
            return await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Group?> GetByIdWithPermissionsAsync(int id)
        {
            return await _context.Groups
                .Include(g => g.GroupPermissions)
                    .ThenInclude(gp => gp.Permission)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        /// <summary>
        /// Adds a new Group entity to the database asynchronously.
        /// </summary>
        public async Task AddAsync(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Replaces existing group permissions with the new list of permission IDs.
        /// </summary>
        public async Task AssignPermissionsAsync(int groupId, List<int> selectedPermissionIds)
        {
            // Fetch existing permissions assigned to this group
            var existingPermissions = await _context.GroupPermissions
                .Where(gp => gp.GroupId == groupId)
                .ToListAsync();

            // Remove existing assignments
            _context.GroupPermissions.RemoveRange(existingPermissions);

            // Add new assignments
            if (selectedPermissionIds != null && selectedPermissionIds.Any())
            {
                var newGroupPermissions = selectedPermissionIds.Select(permissionId => new GroupPermission
                {
                    GroupId = groupId,
                    PermissionId = permissionId
                });

                await _context.GroupPermissions.AddRangeAsync(newGroupPermissions);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Group?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Groups
                .Include(g => g.CreatedBy)
                .Include(g => g.Users)
                .Include(g => g.GroupPermissions)
                    .ThenInclude(gp => gp.Permission)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        /// <summary>
        /// Soft delete: flips IsActive to false instead of removing the row.
        /// </summary>
        public async Task SoftDeleteAsync(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                group.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
        public async Task ActivateAsync(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                group.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }

    }
}