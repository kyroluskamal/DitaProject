using System.ComponentModel.DataAnnotations;
using AppConfgDocumentation.Models;

namespace AppConfgDocumentation.ModelViews
{
    public class DocVersionViewModel : CommonModel
    {
        [Required]
        public string VersionNumber { get; set; } = string.Empty;
        [Required]
        public int DocumentId { get; set; }
    }

    public record DocVersionUpdateViewModel([Required] string VersionNumber);
}