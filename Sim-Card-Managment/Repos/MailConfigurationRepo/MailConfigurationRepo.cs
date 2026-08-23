using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.MailConfigurationRepos
{
    public class MailConfigurationRepo : IMailConfigurationRepo
    {
        private readonly AppDbContext _context;

        public MailConfigurationRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MailConfiguration>> GetAllAsync()
        {
            return await _context.MailConfigurations
                .OrderByDescending(m => m.IsActive)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<MailConfiguration?> GetByIdAsync(int id)
        {
            return await _context.MailConfigurations.FindAsync(id);
        }

        public async Task<MailConfiguration?> GetActiveAsync()
        {
            return await _context.MailConfigurations
                .FirstOrDefaultAsync(m => m.IsActive);
        }

        public async Task AddAsync(MailConfiguration config)
        {
            if (config.IsActive)
                await DeactivateAllAsync();

            config.CreatedAt = DateTime.Now;
            await _context.MailConfigurations.AddAsync(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MailConfiguration config)
        {
            var existing = await _context.MailConfigurations.FindAsync(config.Id);
            if (existing == null) return;

            existing.Name = config.Name;
            existing.SmtpHost = config.SmtpHost;
            existing.SmtpPort = config.SmtpPort;
            existing.SenderEmail = config.SenderEmail;
            existing.SenderDisplayName = config.SenderDisplayName;
            existing.EnableSsl = config.EnableSsl;
            existing.UpdatedAt = DateTime.Now;

            // Only overwrite the stored password if the form actually posted a new
            // one — the Edit view leaves this blank to mean "keep existing".
            if (!string.IsNullOrWhiteSpace(config.SenderPassword))
                existing.SenderPassword = config.SenderPassword;

            if (config.IsActive && !existing.IsActive)
            {
                await DeactivateAllAsync();
                existing.IsActive = true;
            }
            else if (!config.IsActive)
            {
                existing.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.MailConfigurations.FindAsync(id);
            if (config != null)
            {
                _context.MailConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetActiveAsync(int id)
        {
            await DeactivateAllAsync();

            var config = await _context.MailConfigurations.FindAsync(id);
            if (config != null)
            {
                config.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }

        private async Task DeactivateAllAsync()
        {
            var activeConfigs = await _context.MailConfigurations
                .Where(m => m.IsActive)
                .ToListAsync();

            foreach (var c in activeConfigs)
                c.IsActive = false;

            // No SaveChanges here — caller saves once, together with its own change.
        }
    }
}