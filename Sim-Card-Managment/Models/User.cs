using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;



namespace Sim_Card_Managment.Models

{

    public class User

    {

        [Key]

        public Guid Id { get; set; }



        [Required]

        [StringLength(100)]

        public string Username { get; set; } = string.Empty;



        [Required]

        public string PasswordHash { get; set; } = string.Empty;  // bcrypt hashed



        [Required]

        [StringLength(200)]

        [EmailAddress]

        public string Email { get; set; } = string.Empty;



        [Required]

        public Guid GroupId { get; set; }



        public DateTime? LastLogin { get; set; }



        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;



        public DateTime CreatedAt { get; set; } = DateTime.Now;



        // Navigation properties

        [ForeignKey(nameof(GroupId))]

        public virtual Group Group { get; set; } = null!;



        public virtual Employee? Employee { get; set; }



        public virtual ICollection<Subscription> CreatedSubscriptions { get; set; } = new List<Subscription>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public virtual ICollection<DeviceStatus> ReportedStatuses { get; set; } = new List<DeviceStatus>();

        public virtual ICollection<DeviceTransfer> LoggedTransfers { get; set; } = new List<DeviceTransfer>();

        public virtual ICollection<ReceiverSignature> Signatures { get; set; } = new List<ReceiverSignature>();

    }

}

