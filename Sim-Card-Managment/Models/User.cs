using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Sim_Card_Management.Models;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;



namespace Sim_Card_Managment.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
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
        public int GroupId { get; set; }

        public DateTime? LastLogin { get; set; }


        public bool IsActive { get; set; } = true;

        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(GroupId))]

        public virtual Group Group { get; set; } = null!;



        public virtual Employee? Employee { get; set; }


        public virtual ICollection<Group> UserCreatedGroups { get; set; } = new List<Group>();
        public virtual ICollection<Subscription> CreatedSubscriptions { get; set; } = new List<Subscription>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public virtual ICollection<DeviceStatus> ReportedStatuses { get; set; } = new List<DeviceStatus>();

        public virtual ICollection<DeviceTransfer> LoggedTransfers { get; set; } = new List<DeviceTransfer>();

        public virtual ICollection<ReceiverSignature> Signatures { get; set; } = new List<ReceiverSignature>();
        public virtual ICollection<Serial> Serials { get; set; } = new List<Serial>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<DeviceSerialOperation> DeviceSerialOperations { get; set; } = new List<DeviceSerialOperation>();
        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public virtual ICollection<VpnConnection> VpnConnections { get; set; } = new List<VpnConnection>();
        public virtual ICollection<VpnConnectionType> VpnConnectionTypes { get; set; } = new List<VpnConnectionType>();
        public virtual ICollection<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();
        public virtual ICollection<InternetLine> InternetLines { get; set; } = new List<InternetLine>();
    }

}

