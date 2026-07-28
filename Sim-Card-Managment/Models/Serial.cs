using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Models
{
    public class Serial
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string SerialNumber { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User CreatedBy { get; set; }

        public int? SimId { get; set; }

        public virtual Sim? Sim { get; set; }

        public int? UsbId { get; set; }

        public virtual Usb? Usb { get; set; }

        public int DocumentDetailsId { get; set; }

        public virtual DocumentDetails DocumentDetails { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}
