using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.QuoteRepo
{
    public interface IQuotaRepo
    {
        IEnumerable<Quota> GetAll();
        Task<IEnumerable<Quota>> GetQuotasByProviderIdAsync(Guid providerId);
        Quota? GetById(Guid id);
        void Add(Quota quota);
        void Update(Quota quota);
        void Delete(Guid id);
    }
}
