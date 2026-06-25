using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface ISIMRepo
    {
        IEnumerable<Sim> GetAll();
        Sim? GetById(int id);
        void Add(Sim sim);
        void Update(Sim sim);
        void Delete(int id);
    }
}