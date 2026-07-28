using Sim_Card_Managment.Models;
using Sim_Card_Managment.ViewModels;
using System;
using System.Collections.Generic;

namespace Sim_Card_Managment.Repos.EmployeeRepos
{
    public interface IEmployeeRepo
    {
        IEnumerable<Employee> GetAll();
        Employee? GetById(int id);
        void Add(Employee employee);
        void Update(Employee employee);
        void Delete(int id);
        Task<List<PersonListItemViewModel>> GetPeopleListAsync(string status);
        Task<IEnumerable<Employee>> SearchActiveEmployeesAsync(string query);
    }
}