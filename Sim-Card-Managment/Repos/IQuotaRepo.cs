using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IQuotaRepo
    {
        IEnumerable<Quota> GetAll();
        Quota? GetById(Guid id);
        void Add(Quota quota);
        void Update(Quota quota);
        void Delete(Guid id);
    }
}
