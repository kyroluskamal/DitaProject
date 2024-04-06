using System.ComponentModel.DataAnnotations;
using AppConfgDocumentation.Models;

namespace AppConfgDocumentation.ModelViews
{
    public class DitaTopicModelView : CommonModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        [Required]
        public int Type { get; set; }
        [Required]
        public int DocumentId { get; set; }
        public int VersionId { get; set; }
        public string VersionNumber { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public ICollection<StepViewModel> Steps { get; set; } = [];
    }

    public record DitaTopicUpdateViewModel([Required] string Title, int DocumentId, [Required] int Type);


    public class DitaTopicVersionViewModel : CommonModel
    {
        public string ShortDescription { get; set; } = string.Empty;
        public int Type { get; set; }
        public int DitaTopicId { get; set; }
        public string VersionNumber { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public ICollection<StepViewModel> Steps { get; set; } = [];
    }

}
