using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Sim_Card_Managment.Models
{
    public class DocumentType
    {
        public int Id { get;  set; }
        public string Name { get;  set; }
        [MaxLength(50)]
        public string DisplayName { get;  set; }
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
