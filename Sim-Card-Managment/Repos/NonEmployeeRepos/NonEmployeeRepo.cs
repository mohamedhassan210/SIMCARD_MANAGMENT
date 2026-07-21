using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sim_Card_Managment.Repos.NonEmployeeRepos
{
    public class NonEmployeeRepo : INonEmployeeRepo
    {
        private readonly AppDbContext _context;

        public NonEmployeeRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<PersonListItemViewModel>> GetPeopleListAsync()
        {
            return await _context.NonEmployees
                .AsNoTracking()
                .Include(ne => ne.Subscriptions)
                .Select(ne => new PersonListItemViewModel
                {
                    Id = ne.Id,
                    Name = ne.Name,
                    ExtraInfo = ne.Type,
                    PersonType = "Non-Employee",
                    Identifier = ne.ContactInfo,
                    // Counts active subscriptions containing a SIM
                    ActiveSimOnlyCount = ne.Subscriptions.Count(s => s.EndDate == null && s.SimId != null),
                    // Counts active subscriptions containing a USB
                    ActiveUsbCount = ne.Subscriptions.Count(s => s.EndDate == null && s.UsbId != null),
                    StartDate = ne.CreatedAt
                })
                .ToListAsync();
        }
        public IEnumerable<NonEmployee> GetAll()
        {
            return _context.NonEmployees.ToList();
        }

        public NonEmployee? GetById(Guid id)
        {
            return _context.NonEmployees.Find(id);
        }

        public void Add(NonEmployee nonEmployee)
        {
            _context.NonEmployees.Add(nonEmployee);
            _context.SaveChanges();
        }

        public void Update(NonEmployee nonEmployee)
        {
            _context.NonEmployees.Update(nonEmployee);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var ne = GetById(id);
            if (ne != null)
            {
                _context.NonEmployees.Remove(ne);
                _context.SaveChanges();
            }
        }
    }
}
