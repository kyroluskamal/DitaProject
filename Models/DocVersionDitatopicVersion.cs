using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppConfgDocumentation.Models
{
    public class DocVersionDitatopicVersion
    {
        [Key, Column(Order = 0)]
        public int DocVersionId { get; set; }
        [Key, Column(Order = 1)]
        public int DitatopicVersionId { get; set; }
        public virtual DocVersion? DocVersion { get; set; }
        public virtual DitatopicVersion? DitatopicVersion { get; set; }
    }
}