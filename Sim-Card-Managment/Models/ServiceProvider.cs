using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Models
{
    public class ServiceProvider
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // مثل: Vodafone

        [MaxLength(150)]
        public string DisplayName { get; set; } // الاسم المعروض باللغة العربية: فودافون مصر

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<Sim> Sims { get; set; } = new List<Sim>();
        public virtual ICollection<Usb> Usbs { get; set; } = new List<Usb>();
        public virtual ICollection<Quota> Quotas { get; set; }
    }
}