using System.Reflection.Metadata;

namespace Sim_Card_Managment.Models
{
    public class DocumentType
    {
        public Guid Id { get;  set; }
        public string Name { get;  set; }
        public string DisplayName { get;  set; }
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
