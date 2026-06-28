using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface INonEmployeeRepo
    {
        IEnumerable<NonEmployee> GetAll();
        NonEmployee? GetById(Guid id);
        void Add(NonEmployee nonEmployee);
        void Update(NonEmployee nonEmployee);
        void Delete(Guid id);
    }
}
