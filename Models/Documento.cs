using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AppConfgDocumentation.Models
{
    public class Documento : CommonModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [ForeignKey(nameof(AuthorId))]
        public int AuthorId { get; set; }
        public virtual ApplicationUser? Author { get; set; }
        public string FolderName { get; set; } = string.Empty;
        public virtual ICollection<DitaTopic> DitaTopics { get; set; } = [];

        public virtual ICollection<DocVersion> DocVersions { get; set; } = [];
    }

    public class DocVersion : CommonModel
    {
        [Required]
        public string VersionNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey(nameof(Document))]
        public int DocumentId { get; set; }
        public virtual Documento? Document { get; set; }
        public virtual ICollection<DocVersionDitatopicVersion> DitatopicVersions { get; set; } = [];
        public virtual ICollection<DitaTopic> DitaTopics { get; set; } = [];
        public virtual ICollection<DocVersionsRoles> Roles { get; set; } = [];
    }

    public class DocVersionsRoles
    {
        public int RoleId { get; set; }
        public virtual IdentityRole<int> Role { get; set; }
        public int DocVersionId { get; set; }
        public virtual DocVersion DocVersion { get; set; }
        public string DitaMapXml { get; set; } = string.Empty;

        public string DitaMapFilePath { get; set; } = string.Empty;
        public string PDFfilePath { get; set; } = string.Empty;
    }
}
