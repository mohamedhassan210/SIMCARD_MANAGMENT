using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── Core tables ──────────────────────────────────────────────
        public DbSet<Employee> Employees { get; set; }
        public DbSet<NonEmployee> NonEmployees { get; set; }
        public DbSet<Sim> Sims { get; set; }
        public DbSet<Usb> Usbs { get; set; }
        public DbSet<Quota> Quotas { get; set; }
        public DbSet<DeviceAction> Actions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        // ── Tracking tables ──────────────────────────────────────────
        public DbSet<ReceiverSignature> ReceiverSignatures { get; set; }
        public DbSet<DeviceTransfer> DeviceTransfers { get; set; }
        public DbSet<DeviceStatus> DeviceStatuses { get; set; }

        // ── Audit ────────────────────────────────────────────────────
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ── Auth tables ──────────────────────────────────────────────
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<GroupPermission> GroupPermissions { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Composite PK: GroupPermission ─────────────────────────
            modelBuilder.Entity<GroupPermission>()
                .HasKey(gp => new { gp.GroupId, gp.PermissionId });

            // ── Unique indexes ────────────────────────────────────────
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.NationalID).IsUnique();

            modelBuilder.Entity<Sim>()
                .HasIndex(s => s.SerialNumber).IsUnique();

            modelBuilder.Entity<Usb>()
                .HasIndex(u => u.SerialNumber).IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Group>()
                .HasIndex(g => g.Name).IsUnique();

            // ── DeviceTransfer: two FKs to Subscription ───────────────
            modelBuilder.Entity<DeviceTransfer>()
                .HasOne(dt => dt.FromSubscription)
                .WithMany(s => s.DeviceTransfers)
                .HasForeignKey(dt => dt.FromSubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeviceTransfer>()
                .HasOne(dt => dt.NewSubscription)
                .WithMany()
                .HasForeignKey(dt => dt.NewSubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── DeviceStatus: two FKs to Sim ─────────────────────────
            modelBuilder.Entity<DeviceStatus>()
                .HasOne(ds => ds.Sim)
                .WithMany(s => s.DeviceStatuses)
                .HasForeignKey(ds => ds.SimId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeviceStatus>()
                .HasOne(ds => ds.ReplacedBySim)
                .WithMany()
                .HasForeignKey(ds => ds.ReplacedBySimId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── DeviceStatus: two FKs to Usb ─────────────────────────
            modelBuilder.Entity<DeviceStatus>()
                .HasOne(ds => ds.Usb)
                .WithMany(u => u.DeviceStatuses)
                .HasForeignKey(ds => ds.UsbId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeviceStatus>()
                .HasOne(ds => ds.ReplacedByUsb)
                .WithMany()
                .HasForeignKey(ds => ds.ReplacedByUsbId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Subscription: disable cascade on multiple paths ────────
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(s => s.EmpId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.NonEmployee)
                .WithMany(ne => ne.Subscriptions)
                .HasForeignKey(s => s.NonEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.CreatedByUser)
                .WithMany(u => u.CreatedSubscriptions)
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Employee ↔ User (one-to-one) ──────────────────────────
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── DeviceTransfer: restrict cascades ─────────────────────
            modelBuilder.Entity<DeviceTransfer>()
                .HasOne(dt => dt.ToEmployee)
                .WithMany(e => e.ReceivedTransfers)
                .HasForeignKey(dt => dt.ToEmpId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeviceTransfer>()
                .HasOne(dt => dt.CreatedByUser)
                .WithMany(u => u.LoggedTransfers)
                .HasForeignKey(dt => dt.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── AuditLog: restrict cascade ────────────────────────────
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.PerformedByUser)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.PerformedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── ReceiverSignature ─────────────────────────────────────
            modelBuilder.Entity<ReceiverSignature>()
                .HasOne(rs => rs.SignedByUser)
                .WithMany(u => u.Signatures)
                .HasForeignKey(rs => rs.SignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── DeviceStatus: ReportedBy ──────────────────────────────
            modelBuilder.Entity<DeviceStatus>()
                .HasOne(ds => ds.ReportedByUser)
                .WithMany(u => u.ReportedStatuses)
                .HasForeignKey(ds => ds.ReportedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
