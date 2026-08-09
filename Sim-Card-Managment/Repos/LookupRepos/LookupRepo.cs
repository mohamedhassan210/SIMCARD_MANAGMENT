using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;

namespace Sim_Card_Managment.Repos.LookupRepos
{
    public class LookupRepo : ILookupRepo
    {
        private readonly AppDbContext _context;

        public LookupRepo(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<PaymentType>> GetPaymentTypesAsync()
        {
            return await _context.PaymentTypes
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceType>> GetServiceTypesAsync()
        {
            return await _context.ServiceTypes
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<VpnConnectionType>> GetVpnConnectionTypesAsync()
        {
            return await _context.VpnConnectionTypes
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        // =========================
        // PAYMENT TYPE
        // =========================

        public async Task<PaymentType?> GetPaymentTypeByIdAsync(int id)
        {
            return await _context.PaymentTypes
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddPaymentTypeAsync(PaymentType paymentType)
        {
            await _context.PaymentTypes.AddAsync(paymentType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePaymentTypeAsync(PaymentType paymentType)
        {
            _context.PaymentTypes.Update(paymentType);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaymentTypeAsync(int id)
        {
            var paymentType = await _context.PaymentTypes.FindAsync(id);

            if (paymentType == null)
                return;

            _context.PaymentTypes.Remove(paymentType);
            await _context.SaveChangesAsync();
        }

        // =========================
        // SERVICE TYPE
        // =========================

        public async Task<ServiceType?> GetServiceTypeByIdAsync(int id)
        {
            return await _context.ServiceTypes
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddServiceTypeAsync(ServiceType serviceType)
        {
            await _context.ServiceTypes.AddAsync(serviceType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateServiceTypeAsync(ServiceType serviceType)
        {
            _context.ServiceTypes.Update(serviceType);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteServiceTypeAsync(int id)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);

            if (serviceType == null)
                return;

            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();
        }

        // =========================
        // VPN CONNECTION TYPE
        // =========================

        public async Task<VpnConnectionType?> GetVpnConnectionTypeByIdAsync(int id)
        {
            return await _context.VpnConnectionTypes
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddVpnConnectionTypeAsync(VpnConnectionType connectionType)
        {
            await _context.VpnConnectionTypes.AddAsync(connectionType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVpnConnectionTypeAsync(VpnConnectionType connectionType)
        {
            _context.VpnConnectionTypes.Update(connectionType);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVpnConnectionTypeAsync(int id)
        {
            var connectionType =
                await _context.VpnConnectionTypes.FindAsync(id);

            if (connectionType == null)
                return;

            _context.VpnConnectionTypes.Remove(connectionType);
            await _context.SaveChangesAsync();
        }
    }
}