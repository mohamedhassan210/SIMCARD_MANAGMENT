using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class MailConfiguration
    {
        public int Id { get; set; }

        // A friendly label so admins can tell configs apart if there's more than one
        // (e.g. "Primary Gmail Relay", "Backup SMTP").
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string SmtpHost { get; set; } = string.Empty;

        [Required]
        [Range(1, 65535)]
        public int SmtpPort { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string SenderEmail { get; set; } = string.Empty;

        // Stored as-is for now (same trust level as appsettings/user-secrets today).
        // Flag if you want this encrypted at rest later.
        [Required]
        [StringLength(500)]
        public string SenderPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string SenderDisplayName { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;

        // Only one row should be active at a time; SmtpEmailService reads whichever is.
        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}