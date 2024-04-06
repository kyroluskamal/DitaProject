using AppConfgDocumentation.Models;

namespace AppConfgDocumentation.ModelViews
{

    public class StepViewModel : CommonModel
    {
        public int Order { get; set; }
        public string Command { get; set; } = string.Empty;
        public int TaskId { get; set; }
    }
}