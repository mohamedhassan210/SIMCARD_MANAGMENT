using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.InternetLineRepos
{
    public class InternetLineRepo : IInternetLineRepo
    {
        private readonly AppDbContext _context;

        public InternetLineRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InternetLineListItemViewModel>> GetAllAsync()
        {
            return await _context.InternetLines
                .Include(il => il.Branch)
                .Include(il => il.ServiceProvider)
                .Include(il => il.ServiceType)
                .Include(il => il.PaymentType)
                .Include(il => il.RenewalType)
                .Include(il => il.CreatedBy)
                // Soonest-to-renew first drives the dashboard; nulls (no
                // renewal date yet) sort to the bottom instead of the top.
                .OrderBy(il => il.NextRenewalDate ?? DateOnly.MaxValue)
                .ThenBy(il => il.Branch.Name)
                .Select(il => new InternetLineListItemViewModel
                {
                    Id = il.Id,
                    BranchName = il.Branch.Name,
                    ServiceProviderName = il.ServiceProvider.Name,
                    ServiceTypeName = il.ServiceType.Name,
                    PaymentTypeName = il.PaymentType.Name,
                    Bandwidth = il.Bandwidth,
                    PhoneNumber = il.PhoneNumber,
                    Status = il.Status,
                    RenewalTypeName = il.RenewalType != null ? il.RenewalType.Name : string.Empty,
                    NextRenewalDate = il.NextRenewalDate,
                    CreatedByUsername = il.CreatedBy.Username
                })
                .ToListAsync();
        }

        public async Task<InternetLineDetailsViewModel?> GetByIdWithDetailsAsync(int id)
        {
            var line = await _context.InternetLines
                .Include(il => il.Branch)
                .Include(il => il.ServiceProvider)
                .Include(il => il.ServiceType)
                .Include(il => il.PaymentType)
                .Include(il => il.RenewalType)
                .Include(il => il.Sim)
                .Include(il => il.CreatedBy)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (line == null) return null;

            return new InternetLineDetailsViewModel
            {
                Id = line.Id,
                BranchName = line.Branch.Name,
                ServiceProviderName = line.ServiceProvider.Name,
                ServiceTypeName = line.ServiceType.Name,
                PaymentTypeName = line.PaymentType.Name,
                PhoneNumber = line.PhoneNumber,
                Bandwidth = line.Bandwidth,
                RenewalTypeName = line.RenewalType != null ? line.RenewalType.Name : string.Empty,
                LastRenewalDate = line.LastRenewalDate,
                NextRenewalDate = line.NextRenewalDate,
                QuotaGB = line.QuotaGB,
                Status = line.Status,
                Notes = line.Notes,
                SimSerial = line.Sim?.SerialNumber,
                CreatedByUsername = line.CreatedBy.Username
            };
        }

        public async Task<InternetLineEditViewModel?> GetForEditAsync(int id)
        {
            var line = await _context.InternetLines
                .Include(il => il.Sim)
                    .ThenInclude(s => s.ServiceProvider)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (line == null) return null;

            return new InternetLineEditViewModel
            {
                Id = line.Id,
                BranchId = line.BranchId,
                ServiceProviderId = line.ServiceProviderId,
                PaymentTypeId = line.PaymentTypeId,
                ServiceTypeId = line.ServiceTypeId,
                SimId = line.SimId,
                PhoneNumber = line.PhoneNumber,
                SimSerialNumber = line.Sim?.SerialNumber,
                SimProviderName = line.Sim?.ServiceProvider?.Name,
                Bandwidth = line.Bandwidth,
                RenewaltypeId = line.RenewaltypeId,
                LastRenewalDate = line.LastRenewalDate,
                NextRenewalDate = line.NextRenewalDate,
                QuotaGB = line.QuotaGB,
                Status = line.Status,
                Notes = line.Notes
            };
        }

        public async Task<IEnumerable<InternetLineListItemViewModel>> GetByBranchAsync(int branchId)
        {
            return await _context.InternetLines
                .Include(il => il.Branch)
                .Include(il => il.ServiceProvider)
                .Include(il => il.ServiceType)
                .Include(il => il.PaymentType)
                .Include(il => il.RenewalType)
                .Include(il => il.CreatedBy)
                .Where(il => il.BranchId == branchId)
                .OrderBy(il => il.NextRenewalDate ?? DateOnly.MaxValue)
                .Select(il => new InternetLineListItemViewModel
                {
                    Id = il.Id,
                    BranchName = il.Branch.Name,
                    ServiceProviderName = il.ServiceProvider.Name,
                    ServiceTypeName = il.ServiceType.Name,
                    PaymentTypeName = il.PaymentType.Name,
                    Bandwidth = il.Bandwidth,
                    PhoneNumber = il.PhoneNumber,
                    Status = il.Status,
                    RenewalTypeName = il.RenewalType != null ? il.RenewalType.Name : string.Empty,
                    NextRenewalDate = il.NextRenewalDate,
                    CreatedByUsername = il.CreatedBy.Username
                })
                .ToListAsync();
        }

        public async Task AddAsync(InternetLineCreateViewModel model)
        {
            var renewalType = await _context.RenewalTypes.FindAsync(model.RenewaltypeId);
            if (renewalType == null)
                throw new InvalidOperationException($"RenewalType {model.RenewaltypeId} was not found.");

            var lastRenewal = model.LastRenewalDate ?? DateOnly.FromDateTime(DateTime.Now);
            var nextRenewal = model.NextRenewalDate ?? lastRenewal.AddMonths(renewalType.DurationInMonths);

            var line = new InternetLine
            {
                BranchId = model.BranchId,
                ServiceProviderId = model.ServiceProviderId,
                PaymentTypeId = model.PaymentTypeId,
                ServiceTypeId = model.ServiceTypeId,
                SimId = model.SimId,
                PhoneNumber = model.PhoneNumber,
                Bandwidth = model.Bandwidth,
                RenewaltypeId = model.RenewaltypeId,
                LastRenewalDate = lastRenewal,
                NextRenewalDate = nextRenewal,
                QuotaGB = model.QuotaGB,
                Status = model.Status,
                Notes = model.Notes,
                CreatedById = model.CreatedById
            };

            await _context.InternetLines.AddAsync(line);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InternetLineEditViewModel model)
        {
            var line = await _context.InternetLines.FindAsync(model.Id);
            if (line == null) return;

            line.BranchId = model.BranchId;
            line.ServiceProviderId = model.ServiceProviderId;
            line.PaymentTypeId = model.PaymentTypeId;
            line.ServiceTypeId = model.ServiceTypeId;
            line.SimId = model.SimId;
            line.PhoneNumber = model.PhoneNumber;
            line.Bandwidth = model.Bandwidth;
            line.RenewaltypeId = model.RenewaltypeId;

            if (model.LastRenewalDate.HasValue)
                line.LastRenewalDate = model.LastRenewalDate;

            if (model.NextRenewalDate.HasValue)
            {
                line.NextRenewalDate = model.NextRenewalDate;
            }
            else if (model.LastRenewalDate.HasValue && model.RenewaltypeId.HasValue)
            {
                var renewalType = await _context.RenewalTypes.FindAsync(model.RenewaltypeId.Value);
                if (renewalType != null)
                    line.NextRenewalDate = model.LastRenewalDate.Value.AddMonths(renewalType.DurationInMonths);
            }

            line.QuotaGB = model.QuotaGB;
            line.Status = model.Status;
            line.Notes = model.Notes;

            _context.InternetLines.Update(line);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RenewAsync(int id)
        {
            var line = await _context.InternetLines
                .Include(il => il.RenewalType)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (line == null || line.RenewalType == null) return false;

            var today = DateOnly.FromDateTime(DateTime.Now);

            line.LastRenewalDate = today;
            line.NextRenewalDate = today.AddMonths(line.RenewalType.DurationInMonths);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<InternetLineExcelViewModel>> GetForExcelAsync()
        {
            return await _context.InternetLines
                .Include(x => x.Branch)
                .Include(x => x.ServiceProvider)
                .Include(x => x.PaymentType)
                .Include(x => x.ServiceType)
                .Include(x => x.RenewalType)
                .Include(x => x.Sim)
                .GroupBy(x => new
                {
                    x.BranchId,
                    BranchName = x.Branch.Name
                })
                .Select(g => new InternetLineExcelViewModel
                {
                    BranchName = g.Key.BranchName,

                    InternetLines = g.Select(x => new InternetLineExcelItem
                    {
                        ServiceProviderName = x.ServiceProvider.Name,
                        PaymentTypeName = x.PaymentType.Name,
                        ServiceTypeName = x.ServiceType.Name,

                        SimSerialNumber = x.Sim != null
                            ? x.Sim.SerialNumber
                            : null,

                        PhoneNumber = x.PhoneNumber,

                        RenewalTypeName = x.RenewalType != null ? x.RenewalType.Name : string.Empty,
                        NextRenewalDate = x.NextRenewalDate,

                        QuotaGB = x.QuotaGB,

                        Bandwidth = x.Bandwidth,

                        Status = x.Status,

                        Notes = x.Notes
                    }).ToList()
                })
                .OrderBy(x => x.BranchName)
                .ToListAsync();
        }
        public async Task<IEnumerable<InternetLineDashboardItemViewModel>> GetForDashboardAsync()
        {
            return await _context.InternetLines
                .Include(il => il.Branch)
                .Include(il => il.ServiceProvider)
                .Include(il => il.PaymentType)
                .Include(il => il.RenewalType)
                .Where(il => il.Status) // only UP lines
                .OrderBy(il => il.NextRenewalDate ?? DateOnly.MaxValue)
                .ThenBy(il => il.Branch.Name)
                .Select(il => new InternetLineDashboardItemViewModel
                {
                    Id = il.Id,
                    BranchName = il.Branch.Name,
                    ServiceProviderName = il.ServiceProvider.Name,
                    PaymentTypeName = il.PaymentType.Name,
                    PhoneNumber = il.PhoneNumber,
                    LastRenewalDate = il.LastRenewalDate,
                    NextRenewalDate = il.NextRenewalDate,
                    RenewalTypeName = il.RenewalType != null ? il.RenewalType.Name : null
                })
                .ToListAsync();
        }

    }
}