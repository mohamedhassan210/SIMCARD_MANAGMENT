using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.SubscriptionRepo.SubscriptionRepo
{
    public interface ISubscriptionRepo
    {
        IEnumerable<Subscription> GetAll();
        Subscription? GetById(Guid id);
        void Add(Subscription subscription);
        void Update(Subscription subscription);
        void Delete(Guid id);
    }
}
