using AppConfgDocumentation.Models;

namespace AppConfgDocumentation.ModelViews
{
    public class DucumentModelView : CommonModel
    {
        public string Title { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string VersionNumber { get; set; } = string.Empty;
    }
}
