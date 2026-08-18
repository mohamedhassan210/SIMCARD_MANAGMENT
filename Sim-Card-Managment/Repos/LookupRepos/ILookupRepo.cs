using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Repos.LookupRepos
{
    public interface ILookupRepo
    {
        // Get
        Task<IEnumerable<PaymentType>> GetPaymentTypesAsync();
        Task<IEnumerable<ServiceType>> GetServiceTypesAsync();
        Task<IEnumerable<VpnConnectionType>> GetVpnConnectionTypesAsync();

        // Payment Type
        Task<PaymentType?> GetPaymentTypeByIdAsync(int id);
        Task AddPaymentTypeAsync(PaymentType paymentType);
        Task UpdatePaymentTypeAsync(PaymentType paymentType);
        Task DeletePaymentTypeAsync(int id);

        // Service Type
        Task<ServiceType?> GetServiceTypeByIdAsync(int id);
        Task AddServiceTypeAsync(ServiceType serviceType);
        Task UpdateServiceTypeAsync(ServiceType serviceType);
        Task DeleteServiceTypeAsync(int id);

        // VPN Connection Type
        Task<VpnConnectionType?> GetVpnConnectionTypeByIdAsync(int id);
        Task AddVpnConnectionTypeAsync(VpnConnectionType connectionType);
        Task UpdateVpnConnectionTypeAsync(VpnConnectionType connectionType);
        Task DeleteVpnConnectionTypeAsync(int id);

        Task<IEnumerable<RenewalType>> GetRenewalTypesAsync();
    }
}