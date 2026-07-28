using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sim_Card_Managment.Models;

namespace Sim_Card_Management.Models
{
    public class DocumentDetails
    {
        [Key]
       public int Id { get; set; }
        
        public int Quantity { get; set; }
        public ICollection<Serial> Serials { get; set; } = new List<Serial>();
        public int ItemTypeId { get; set; }
        [ForeignKey(nameof(ItemTypeId))]
        public ItemType ItemType { get; set; }
        public int DocumentId { get; set; }
        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }


    }
}
