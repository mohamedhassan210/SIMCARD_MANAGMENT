using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Models
{
    public class ServiceProvider
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(150)]
        public string DisplayName { get; set; }
        public bool IsActive { get; set; } = true;

        // NEW — relative path under wwwroot, e.g. "/uploads/logos/3.png"
        [MaxLength(300)]
        public string? LogoPath { get; set; }

        // NEW — comma-separated phone number prefixes this provider owns, e.g. "010,0100,0101"
        [MaxLength(200)]
        public string? PhonePrefixes { get; set; }

        public virtual ICollection<Quota> Quotas { get; set; } = new List<Quota>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<Sim> Sims { get; set; } = new List<Sim>();
        public virtual ICollection<Usb> Usbs { get; set; } = new List<Usb>();
        public virtual ICollection<InternetLine> InternetLines { get; set; } = new List<InternetLine>();
        public virtual ICollection<VpnConnection> VpnConnections { get; set; } = new List<VpnConnection>();

    }
}