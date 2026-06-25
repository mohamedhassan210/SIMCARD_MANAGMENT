using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.SIMRepo.SIMRepo
{
    public interface ISIMRepo
    {
        IEnumerable<Sim> GetAll();
        Sim? GetById(Guid id);
        void Add(Sim sim);
        void Update(Sim sim);
        void Delete(Guid id);
    }
}