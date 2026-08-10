using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.VpnConnectionRepos
{
    public class VpnConnectionRepo : IVpnConnectionRepo
    {
        private readonly AppDbContext _context;

        public VpnConnectionRepo(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================
        public async Task<IEnumerable<VpnConnectionListItemViewModel>> GetAllAsync()
        {
            return await _context.VpnConnections
                .Include(v => v.Branch)
                .Include(v => v.ConnectionType)
                .Include(v => v.ServiceProvider)
                .Include(v => v.CreatedBy)
                .OrderBy(v => v.Branch.Name)
                .Select(v => new VpnConnectionListItemViewModel
                {
                    Id = v.Id,

                    BranchName = v.Branch.Name,
                    ConnectionTypeName = v.ConnectionType.Name,
                    ServiceProviderName = v.ServiceProvider.Name,

                    NID = v.NID,
                    LineSpeed = v.LineSpeed,
                    Status = v.Status,

                   
                    CreatedByUsername = v.CreatedBy.Username
                })
                .ToListAsync();
        }

        // =========================
        // GET DETAILS
        // =========================
        public async Task<VpnConnectionDetailsViewModel?> GetByIdWithDetailsAsync(int id)
        {
            var vpn = await _context.VpnConnections
                .Include(v => v.Branch)
                .Include(v => v.ConnectionType)
                .Include(v => v.ServiceProvider)
                .Include(v => v.CreatedBy)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vpn == null)
                return null;

            return new VpnConnectionDetailsViewModel
            {
                Id = vpn.Id,

                BranchName = vpn.Branch.Name,
                ConnectionTypeName = vpn.ConnectionType.Name,
                ServiceProviderName = vpn.ServiceProvider.Name,

                NID = vpn.NID,
                LineSpeed = vpn.LineSpeed,
                Status = vpn.Status,
                Notes = vpn.Notes,

               
                CreatedByUsername = vpn.CreatedBy.Username
            };
        }

        // =========================
        // GET FOR EDIT
        // =========================
        public async Task<VpnConnectionEditViewModel?> GetForEditAsync(int id)
        {
            var vpn = await _context.VpnConnections.FindAsync(id);

            if (vpn == null)
                return null;

            return new VpnConnectionEditViewModel
            {
                Id = vpn.Id,

                BranchId = vpn.BranchId,
                ConnectionTypeId = vpn.ConnectionTypeId,
                ServiceProviderId = vpn.ServiceProviderId,

                NID = vpn.NID,
                LineSpeed = vpn.LineSpeed,
                Status = vpn.Status,
                Notes = vpn.Notes
            };
        }

        // =========================
        // GET BY BRANCH
        // =========================
        public async Task<IEnumerable<VpnConnectionListItemViewModel>> GetByBranchAsync(int branchId)
        {
            return await _context.VpnConnections
                .Include(v => v.Branch)
                .Include(v => v.ConnectionType)
                .Include(v => v.ServiceProvider)
                .Include(v => v.CreatedBy)
                .Where(v => v.BranchId == branchId)
                .Select(v => new VpnConnectionListItemViewModel
                {
                    Id = v.Id,

                    BranchName = v.Branch.Name,
                    ConnectionTypeName = v.ConnectionType.Name,
                    ServiceProviderName = v.ServiceProvider.Name,

                    NID = v.NID,
                    LineSpeed = v.LineSpeed,
                    Status = v.Status,

                    
                    CreatedByUsername = v.CreatedBy.Username
                })
                .ToListAsync();
        }

        // =========================
        // CREATE
        // =========================
        public async Task AddAsync(VpnConnectionCreateViewModel model)
        {
            var vpn = new VpnConnection
            {
                BranchId = model.BranchId,
                ConnectionTypeId = model.ConnectionTypeId,
                ServiceProviderId = model.ServiceProviderId,

                NID = model.NID,
                LineSpeed = model.LineSpeed,
                Status = model.Status,
                Notes = model.Notes,

                CreatedById = model.CreatedById
            };

            await _context.VpnConnections.AddAsync(vpn);
            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE
        // =========================
        public async Task UpdateAsync(VpnConnectionEditViewModel model)
        {
            var vpn = await _context.VpnConnections.FindAsync(model.Id);

            if (vpn == null)
                return;

            vpn.BranchId = model.BranchId;
            vpn.ConnectionTypeId = model.ConnectionTypeId;
            vpn.ServiceProviderId = model.ServiceProviderId;

            vpn.NID = model.NID;
            vpn.LineSpeed = model.LineSpeed;
            vpn.Status = model.Status;
            vpn.Notes = model.Notes;

            // We intentionally DON'T change:
            // vpn.CreatedById
            // vpn.CreatedAt

            _context.VpnConnections.Update(vpn);
            await _context.SaveChangesAsync();
        }


        public async Task<List<VpnExcelBranch>> GetForExcelAsync()
        {
            var branches = await _context.Branches
                .Include(b => b.VpnConnections)
                    .ThenInclude(v => v.ConnectionType)
                .Include(b => b.VpnConnections)
                    .ThenInclude(v => v.ServiceProvider)
                .ToListAsync();

            return branches.Select(branch => new VpnExcelBranch
            {
                BranchName = branch.Name,

                IsActive = branch.IsActive,

                VpnOverInternetStatus = branch.VpnOverInternetStatus,

                Connections = branch.VpnConnections
                    .Select(v => new VpnExcelConnection
                    {
                        ConnectionTypeName = v.ConnectionType.Name,

                        ServiceProviderName = v.ServiceProvider.Name,

                        NID = v.NID,

                        LineSpeed = v.LineSpeed,

                        Status = v.Status,

                        Notes = v.Notes
                    })
                    .ToList(),

                Notes = string.Join(
                    Environment.NewLine,
                    branch.VpnConnections
                        .Where(v => !string.IsNullOrWhiteSpace(v.Notes))
                        .Select(v => v.Notes)
                        .Distinct()
                )
            }).ToList();
        }

    }
}