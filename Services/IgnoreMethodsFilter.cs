using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppConfgDocumentation.Services
{
    public class IgnoreMethodsFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathsToRemove = swaggerDoc.Paths
                .Where(pathItem => pathItem.Key.Contains("GeneratePDF")) // Adjust the condition as needed
                .ToList();

            foreach (var item in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(item.Key);
            }
        }
    }
}