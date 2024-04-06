using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace AppConfgDocumentation.Services
{
    public interface IDitaValidationService
    {
        bool ValidateDitaXml(string xmlContent, out string validationErrors);
    }

    // DitaValidationService.cs
    public class DitaValidationService : IDitaValidationService
    {
        // private readonly XmlSchemaSet schemas;
        // private string baseSchemaPath = @"C:\dita\plugins\org.oasis-open.dita.v2_0\dtd";
        // public DitaValidationService()
        // {
        //     // schemas = new XmlSchemaSet();
        //     // AddSchemaToSet(@$"{baseSchemaPath}\base\", "basemap.dtd");
        //     // AddSchemaToSet(@$"{baseSchemaPath}\base\", "basetopic.dtd");
        //     // AddSchemaToSet(@$"{baseSchemaPath}\subjectScheme\", "subjectScheme.dtd");
        //     // schemas.Compile();
        // }

        // private void AddSchemaToSet(string baseSchemaPath, string schemaFileName)
        // {
        //     var settings = new XmlReaderSettings
        //     {
        //         DtdProcessing = DtdProcessing.Parse,
        //         ValidationType = ValidationType.DTD
        //     };

        //     var schemaPath = Path.Combine(baseSchemaPath, schemaFileName);
        //     using var schemaReader = XmlReader.Create(schemaPath, settings);

        //     var schema = XmlSchema.Read(schemaReader, ValidationEventHandler);
        //     if (schema != null)
        //     {
        //         schemas.Add(schema);
        //     }
        // }

        public bool ValidateDitaXml(string xmlContent, out string validationErrors)
        {
            // This local variable can be modified within the lambda expression.
            var errors = new StringBuilder();

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,

                ValidationType = ValidationType.DTD,
                ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints |
                                  XmlSchemaValidationFlags.ReportValidationWarnings
            };

            settings.ValidationEventHandler += (sender, e) =>
            {
                errors.AppendLine($"{e.Severity}: {e.Message}");
            };

            try
            {
                // Parse and validate the XML content
                using var xmlReader = XmlReader.Create(new StringReader(xmlContent), settings);
                while (xmlReader.Read()) { }
            }
            catch (XmlException ex)
            {
                errors.AppendLine($"Exception: {ex.Message}");
                validationErrors = errors.ToString();
                return false;
            }

            validationErrors = errors.ToString();
            return string.IsNullOrEmpty(validationErrors);
        }

        private static void ValidationEventHandler(object? sender, ValidationEventArgs e)
        {
            // This is where you handle any validation errors encountered
            // during the loading of the schemas
            if (e.Severity == XmlSeverityType.Warning)
            {
                Console.WriteLine($"WARNING: {e.Message}");
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                throw new Exception($"ERROR: {e.Message}");
            }
        }
    }
}
