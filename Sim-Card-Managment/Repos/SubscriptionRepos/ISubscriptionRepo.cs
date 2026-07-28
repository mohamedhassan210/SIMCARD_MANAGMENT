using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface ISubscriptionRepo
    {
        Task<IEnumerable<Subscription>> GetAllSubscriptionsWithDetailsAsync();
        IEnumerable<Subscription> GetAll();
        Subscription? GetById(Guid id);
        void Add(Subscription subscription);   
        void Update(Subscription subscription);
        void Delete(Guid id);
        Task<IEnumerable<Subscription>> GetAllWithHardwareDetailsAsync();
    }
}
