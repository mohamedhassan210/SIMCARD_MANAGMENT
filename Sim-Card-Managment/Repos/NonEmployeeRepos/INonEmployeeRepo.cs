using Sim_Card_Managment.Models;
using Sim_Card_Managment.ViewModels;

namespace Sim_Card_Managment.Repos.NonEmployeeRepos
{
    public interface INonEmployeeRepo
    {
        IEnumerable<NonEmployee> GetAll();
        NonEmployee? GetById(Guid id);
        void Add(NonEmployee nonEmployee);
        void Update(NonEmployee nonEmployee);
        void Delete(Guid id);
        Task<List<PersonListItemViewModel>> GetPeopleListAsync();

        // Add this method:
        Task<IEnumerable<NonEmployee>> SearchNonEmployeesAsync(string query);
        IEnumerable<string> GetDistinctTypes();
    }
}