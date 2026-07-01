using Sim_Card_Managment.Models;
using System;
using System.Collections.Generic;

namespace Sim_Card_Managment.Repos.EmployeeRepos
{
    public interface IEmployeeRepo
    {
        IEnumerable<Employee> GetAll();
        Employee? GetById(Guid id);
        void Add(Employee employee);
        void Update(Employee employee);
        void Delete(Guid id);
    }
}