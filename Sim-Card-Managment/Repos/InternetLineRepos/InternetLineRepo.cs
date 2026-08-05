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
                .OrderBy(il => il.Branch.Name)
                .Select(il => new InternetLineListItemViewModel
                {
                    Id = il.Id,
                    BranchName = il.Branch.Name,
                    ServiceProviderName = il.ServiceProvider.Name,
                    ServiceTypeName = il.ServiceType.Name,
                    PaymentTypeName = il.PaymentType.Name,
                    Bandwidth = il.Bandwidth,
                    PhoneNumber = il.PhoneNumber,
                    Status = il.Status
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
                .Include(il => il.Sim)
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
                RenewalDay = line.RenewalDay,
                QuotaGB = line.QuotaGB,
                Status = line.Status,
                Notes = line.Notes,
                SimSerial = line.Sim?.SerialNumber
            };
        }

        public async Task<InternetLineEditViewModel?> GetForEditAsync(int id)
        {
            var line = await _context.InternetLines.FindAsync(id);
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
                Bandwidth = line.Bandwidth,
                RenewalDay = line.RenewalDay,
                QuotaGB = line.QuotaGB,
                Status = line.Status,
                Notes = line.Notes
            };
        }

        public async Task<IEnumerable<InternetLineListItemViewModel>> GetByBranchAsync(int branchId)
        {
            return await _context.InternetLines
                .Include(il => il.ServiceProvider)
                .Include(il => il.ServiceType)
                .Include(il => il.PaymentType)
                .Where(il => il.BranchId == branchId)
                .Select(il => new InternetLineListItemViewModel
                {
                    Id = il.Id,
                    BranchName = il.Branch.Name,
                    ServiceProviderName = il.ServiceProvider.Name,
                    ServiceTypeName = il.ServiceType.Name,
                    PaymentTypeName = il.PaymentType.Name,
                    Bandwidth = il.Bandwidth,
                    PhoneNumber = il.PhoneNumber,
                    Status = il.Status
                })
                .ToListAsync();
        }

        public async Task AddAsync(InternetLineCreateViewModel model)
        {
            var line = new InternetLine
            {
                BranchId = model.BranchId,
                ServiceProviderId = model.ServiceProviderId,
                PaymentTypeId = model.PaymentTypeId,
                ServiceTypeId = model.ServiceTypeId,
                SimId = model.SimId,
                PhoneNumber = model.PhoneNumber,
                Bandwidth = model.Bandwidth,
                RenewalDay = model.RenewalDay,
                QuotaGB = model.QuotaGB,
                Status = model.Status,
                Notes = model.Notes
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
            line.RenewalDay = model.RenewalDay;
            line.QuotaGB = model.QuotaGB;
            line.Status = model.Status;
            line.Notes = model.Notes;

            _context.InternetLines.Update(line);
            await _context.SaveChangesAsync();
        }
    }
}