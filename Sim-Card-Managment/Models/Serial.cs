using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Sim_Card_Managment.Models
{
    public class Serial
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string SerialNumber { get; set; }

        public Guid UserId { get; set; }

        public User CreatedBy { get; set; }

        public Guid? SimId { get; set; }

        public virtual Sim? Sim { get; set; }

        public Guid? UsbId { get; set; }

        public virtual Usb? Usb { get; set; }

        public Guid DocumentId { get; set; }

        public virtual Document Document { get; set; }


        public DateTime CreatedDate { get; set; }

    }
}
