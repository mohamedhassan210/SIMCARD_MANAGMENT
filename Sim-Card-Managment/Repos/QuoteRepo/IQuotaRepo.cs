using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.QuoteRepo
{
    public interface IQuotaRepo
    {
        IEnumerable<Quota> GetAll();
        Task<IEnumerable<Quota>> GetQuotasByProviderIdAsync(int providerId);
        Quota? GetById(int id);
        void Add(Quota quota);
        void Update(Quota quota);
        void Delete(int id);
    }
}
