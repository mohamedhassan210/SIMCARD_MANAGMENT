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

            var normalizedStatus = status?.ToLower().Trim();

            if (normalizedStatus == "active")
                query = query.Where(e => e.IsActive);
            else if (normalizedStatus == "inactive")
                query = query.Where(e => !e.IsActive);

            var now = DateTime.Now;

            return await query
                .Select(e => new PersonListItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    ExtraInfo = e.Position,
                    PersonType = "Employee",
                    Identifier = e.EmpCode,
                    IsActive = e.IsActive, // Ensure IsActive is mapped to the view model
                    ActiveSimOnlyCount = e.Subscriptions.Count(s => s.SimId != null && (s.EndDate == null || s.EndDate >= now)),
                    ActiveUsbCount = e.Subscriptions.Count(s => s.UsbId != null && (s.EndDate == null || s.EndDate >= now)),
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

        public Employee? GetById(int id)
        {
            return _context.Employees
                .Include(e => e.Subscriptions!)
                    .ThenInclude(s => s.Sim)
                        .ThenInclude(s=>s.ServiceProvider)
                .Include(e => e.Subscriptions!)
                    .ThenInclude(s => s.Usb)
                .Include(e => e.Subscriptions!)
                    .ThenInclude(s => s.Quota)
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

        public void Delete(int id)
        {
            // Find all subscriptions associated with this employee
            var subscriptions = _context.Subscriptions
                .Where(s => s.EmpId == id)
                .ToList();

            if (subscriptions.Any())
            {
                _context.Subscriptions.RemoveRange(subscriptions);
            }

            var employee = _context.Employees.Find(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                _context.SaveChanges(); // Saves both subscription removals and employee deletion
            }
        }
    }
}