using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.data
{
    public class AppContext :DbContext
    {
        public AppContext(DbContextOptions options) : base(options) { }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<NonEmployee> NonEmployees { get; set; }
        public virtual DbSet<Sim_Card_Managment.Models.Action> Actions { get; set; }
        public virtual DbSet<Status> Statuss { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<SIM> SIMs { get; set; }
        public virtual DbSet<USB> USBs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=COM168-LAB3\\SQLEXPRESS;Initial Catalog=SimCardDB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<USB>().HasIndex(u => u.SerialNumber).IsUnique();
        }
    }
}
