using Sim_Card_Managment.Models;


namespace Sim_Card_Managment.Repos
{
    public interface ISIMRepo
    {
        Task<IEnumerable<Sim>> GetAvailableSimsAsync();
        IEnumerable<Sim> GetAll();
        Sim? GetById(Guid id);
        void Add(Sim sim);
        void Update(Sim sim);
        void Delete(Guid id);
    }
}