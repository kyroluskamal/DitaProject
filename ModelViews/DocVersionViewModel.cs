using System.ComponentModel.DataAnnotations;
using AppConfgDocumentation.Models;

namespace AppConfgDocumentation.ModelViews
{
    public class DocVersionViewModel : CommonModel
    {
        [Required]
        public string VersionNumber { get; set; } = string.Empty;
        public string DitaMapXml { get; set; } = string.Empty;

        public string DitaMapFilePath { get; set; } = string.Empty;
        [Required]
        public int DocumentId { get; set; }
    }

}