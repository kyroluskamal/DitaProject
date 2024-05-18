using System.ComponentModel.DataAnnotations;
using AppConfgDocumentation.Models;
using Microsoft.AspNetCore.Identity;

namespace AppConfgDocumentation.ModelViews
{
    public class DitaTopicModelView : CommonModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public int DocFamilyId { get; set; }
        public bool IsRequired { get; set; } = false;

    }



    public class DitaTopicVersionViewModel : CommonModel
    {
        public string ShortDescription { get; set; } = string.Empty;
        public int Type { get; set; }
        public int DocFamilyId { get; set; }

        public int DitaTopicId { get; set; }
        [Required]
        public ICollection<int> Roles { get; set; } = [];
        public string VersionNumber { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public ICollection<StepViewModel> Steps { get; set; } = [];
    }

}
