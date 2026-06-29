using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sim_Card_Managment.Repos
{
    public class NonEmployeeRepo : INonEmployeeRepo
    {
        private readonly AppDbContext _context;

        public NonEmployeeRepo(AppDbContext context)
        {
            _context = context;
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
