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

        public async Task<IEnumerable<PaymentType>> GetPaymentTypesAsync()
        {
            return await _context.PaymentTypes.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<IEnumerable<ServiceType>> GetServiceTypesAsync()
        {
            return await _context.ServiceTypes.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<IEnumerable<VpnConnectionType>> GetVpnConnectionTypesAsync()
        {
            return await _context.VpnConnectionTypes.OrderBy(v => v.Name).ToListAsync();
        }
    }
}