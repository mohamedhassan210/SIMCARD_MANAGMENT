using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Repos.LookupRepos
{
    public interface ILookupRepo
    {
        Task<IEnumerable<PaymentType>> GetPaymentTypesAsync();
        Task<IEnumerable<ServiceType>> GetServiceTypesAsync();
        Task<IEnumerable<VpnConnectionType>> GetVpnConnectionTypesAsync();
    }
}