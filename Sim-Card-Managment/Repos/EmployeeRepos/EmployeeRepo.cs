using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sim_Card_Managment.Repos.EmployeeRepos
{
    public class EmployeeRepo : IEmployeeRepo
    {
        private readonly AppDbContext _context;

        public EmployeeRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Employee>> SearchActiveEmployeesAsync(string query)
        {
            return await _context.Employees
                .Where(e => e.IsActive && (e.Name.Contains(query) || e.NationalID.Contains(query)))
                .Take(6)
                .ToListAsync();
        }
        public async Task<List<PersonListItemViewModel>> GetPeopleListAsync(string status)
        {
            var query = _context.Employees
                .AsNoTracking()
                .Include(e => e.Subscriptions)
                .AsQueryable();

            if (status == "active")
                query = query.Where(e => e.IsActive);
            else if (status == "inactive")
                query = query.Where(e => !e.IsActive);

            return await query
                .Select(e => new PersonListItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    ExtraInfo = e.Position,
                    PersonType = "Employee",
                    Identifier = e.NationalID,
                    // Counts active subscriptions containing a SIM
                    ActiveSimOnlyCount = e.Subscriptions.Count(s => s.EndDate == null && s.SimId != null),
                    // Counts active subscriptions containing a USB
                    ActiveUsbCount = e.Subscriptions.Count(s => s.EndDate == null && s.UsbId != null),
                    StartDate = e.CreatedAt
                })
                .ToListAsync();
        }
        public IEnumerable<Employee> GetAll()
        {
            // Êã ÅÖÇÝÉ Include áÌÏæá ÇáÇÔÊÑÇßÇÊ áßí íÚãá ÇáÜ Count ÇáÏíäÇãíßí Ýí ÇáÜ View
            return _context.Employees
                           .Include(e => e.User)
                           .Include(e => e.Subscriptions)
                           .ToList();
        }

        public Employee? GetById(Guid id)
        {
            return _context.Employees
                .Include(e => e.Subscriptions)
                .Include(e => e.ReceivedTransfers)
                .FirstOrDefault(e => e.Id == id);
        }

        public void Add(Employee employee)
        {
            string nid = employee.NationalID;
            var x = _context.Employees.FirstOrDefault(e => e.NationalID == nid);

            if (x != null)
            {
                return;
            }

            _context.Employees.Add(employee);
            _context.SaveChanges();
        }

        public void Update(Employee employee)
        {
            _context.Employees.Update(employee);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var employee = _context.Employees.Find(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                _context.SaveChanges();
            }
        }
    }
}