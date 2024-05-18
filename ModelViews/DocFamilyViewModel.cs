using AppConfgDocumentation.Models;

namespace AppConfgDocumentation.ModelViews
{
    public class DocFamilyViewModel : CommonModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
    }
}
