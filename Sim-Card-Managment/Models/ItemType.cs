using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Management.Models
{
    public class ItemType
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public ICollection<DocumentDetails> DocumentDetails { get; set; } = new List<DocumentDetails>();
    }
}
