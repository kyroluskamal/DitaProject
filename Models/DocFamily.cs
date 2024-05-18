using System.ComponentModel.DataAnnotations.Schema;

namespace AppConfgDocumentation.Models;

public class DocFamily : CommonModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public virtual ICollection<DitaTopic> DitaTopics { get; set; } = [];
    public virtual ICollection<Documento> Documentos { get; set; } = [];
}


