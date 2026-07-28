using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface ISubscriptionRepo
    {
        Task<IEnumerable<Subscription>> GetAllSubscriptionsWithDetailsAsync();
        IEnumerable<Subscription> GetAll();
        Subscription? GetById(int id);
        void Add(Subscription subscription);   
        void Update(Subscription subscription);
        void Delete(int id);
        Task<IEnumerable<Subscription>> GetAllWithHardwareDetailsAsync();
    }
}
