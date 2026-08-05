using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.BranchRepos
{
    public class BranchRepo : IBranchRepo
    {
        private readonly AppDbContext _context;

        public BranchRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BranchListItemViewModel>> GetAllAsync()
        {
            return await _context.Branches
                .Include(b => b.InternetLines)
                .Include(b => b.VpnConnections)
                .OrderBy(b => b.Name)
                .Select(b => new BranchListItemViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    IsActive = b.IsActive,
                    VpnOverInternetStatus = b.VpnOverInternetStatus,
                    CreatedAt = b.CreatedAt,
                    InternetLineCount = b.InternetLines.Count,
                    VpnConnectionCount = b.VpnConnections.Count
                })
                .ToListAsync();
        }

        public async Task<BranchDetailsViewModel?> GetByIdWithDetailsAsync(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.InternetLines)
                    .ThenInclude(il => il.ServiceProvider)
                .Include(b => b.InternetLines)
                    .ThenInclude(il => il.ServiceType)
                .Include(b => b.InternetLines)
                    .ThenInclude(il => il.PaymentType)
                .Include(b => b.VpnConnections)
                    .ThenInclude(v => v.ConnectionType)
                .Include(b => b.VpnConnections)
                    .ThenInclude(v => v.ServiceProvider)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null) return null;

            return new BranchDetailsViewModel
            {
                Id = branch.Id,
                Name = branch.Name,
                IsActive = branch.IsActive,
                VpnOverInternetStatus = branch.VpnOverInternetStatus,
                CreatedAt = branch.CreatedAt,
                InternetLines = branch.InternetLines.Select(il => new InternetLineListItemViewModel
                {
                    Id = il.Id,
                    BranchName = branch.Name,
                    ServiceProviderName = il.ServiceProvider?.Name ?? "N/A",
                    ServiceTypeName = il.ServiceType?.Name ?? "N/A",
                    PaymentTypeName = il.PaymentType?.Name ?? "N/A",
                    Bandwidth = il.Bandwidth,
                    PhoneNumber = il.PhoneNumber,
                    Status = il.Status
                }).ToList(),
                VpnConnections = branch.VpnConnections.Select(v => new VpnConnectionListItemViewModel
                {
                    Id = v.Id,
                    BranchName = branch.Name,
                    ConnectionTypeName = v.ConnectionType?.Name ?? "N/A",
                    ServiceProviderName = v.ServiceProvider?.Name ?? "N/A",
                    NID = v.NID,
                    LineSpeed = v.LineSpeed,
                    Status = v.Status
                }).ToList()
            };
        }

        public async Task<BranchEditViewModel?> GetForEditAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return null;

            return new BranchEditViewModel
            {
                Id = branch.Id,
                Name = branch.Name,
                VpnOverInternetStatus = branch.VpnOverInternetStatus
            };
        }

        public async Task AddAsync(BranchCreateViewModel model)
        {
            var branch = new Branch
            {
                Name = model.Name,
                VpnOverInternetStatus = model.VpnOverInternetStatus,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            await _context.Branches.AddAsync(branch);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BranchEditViewModel model)
        {
            var branch = await _context.Branches.FindAsync(model.Id);
            if (branch == null) return;

            branch.Name = model.Name;
            branch.VpnOverInternetStatus = model.VpnOverInternetStatus;
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                branch.IsActive = false;
                _context.Branches.Update(branch);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ActivateAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                branch.IsActive = true;
                _context.Branches.Update(branch);
                await _context.SaveChangesAsync();
            }
        }
    }
}