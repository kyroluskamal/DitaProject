namespace AppConfgDocumentation.Models
{
    public class ReturnTypeOfDitaToics
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public dynamic? Ditatopic { get; set; }
        public DitatopicVersion? DitatopicVersion { get; set; }
    }
}