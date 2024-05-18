using AppConfgDocumentation.Data;
using AppConfgDocumentation.Models;
using AppConfgDocumentation.ModelViews;
using AppConfgDocumentation.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppConfgDocumentation.Controllers
{
    public class DocFamilyController : Controller_Base
    {
        private readonly ApplicationDbContext _dbContext;
        // private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDitaFileCreationService _ditaFileCreationService;

        // private readonly RoleManager<IdentityRole<int>> _roleManager;
        public DocFamilyController(ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment,
            IDitaFileCreationService ditaFileCreationService,
         RoleManager<IdentityRole<int>> roleManager)
        {
            _dbContext = dbContext;
            _ditaFileCreationService = ditaFileCreationService;
            // _hostingEnvironment = hostingEnvironment;
            // _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetFamilies()
        {
            var families = await _dbContext.DocFamilies.Include(x => x.DitaTopics)
            .ThenInclude(dt => dt.DitatopicVersions)
            .Include(c => c.Documentos)
            .Select(f => new
            {
                f.Id,
                f.Title,
                f.Description,
                DitaTopics = f.DitaTopics.Select(dt => new
                {
                    dt.Id,
                    dt.Title,
                    dt.DocFamilyId,
                    dt.IsRequired,
                    DitatopicVersions = dt.DitatopicVersions.Select(dv => new
                    {
                        dv.Id,
                        dv.ShortDescription,
                        dv.FileName,
                        dv.VersionNumber,
                        dv.CreatedAt,
                        dv.DitaTopicId,
                        Type = dv is ConceptVersion ? 0 : dv is TaskVersion ? 1 : -1,
                        body = dv is ConceptVersion ? ((ConceptVersion)dv).Body : null,
                        Roles = dv.Roles.Select(x =>
                                    x.RoleId
                                ).ToList(),
                        steps = dv is TaskVersion ? ((TaskVersion)dv).Steps.Select(s => new
                        {
                            s.Id,
                            s.Order,
                            s.Command,
                            s.TaskVersionId,
                        }) : null
                    }),
                    Roles = dt.DitatopicVersions.SelectMany(x => x.Roles).Select(x => new
                    {
                        x.RoleId,
                        x.Role.Name,
                    }).ToList(),
                }).ToList(),
                Documentos = f.Documentos.Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.DocFamilyId,
                    d.AuthorId,
                    d.FolderName,
                }).ToList(),
            }).ToListAsync();
            return Ok(families);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFamiliyById(int id)
        {
            var family = await _dbContext.DocFamilies.Include(x => x.DitaTopics)
            .ThenInclude(dt => dt.DitatopicVersions).Include(c => c.Documentos)
            .Select(f => new
            {
                f.Id,
                f.Title,
                f.Description,
                DitaTopics = f.DitaTopics.Select(dt => new
                {
                    dt.Id,
                    dt.Title,
                    dt.DocFamilyId,
                    dt.IsRequired,
                    DitatopicVersions = dt.DitatopicVersions.Select(dv => new
                    {
                        dv.Id,
                        dv.ShortDescription,
                        dv.FileName,
                        dv.VersionNumber,
                        dv.CreatedAt,
                        dv.DitaTopicId,
                        Type = dv is ConceptVersion ? 0 : dv is TaskVersion ? 1 : -1,
                        body = dv is ConceptVersion ? ((ConceptVersion)dv).Body : null,
                        Roles = dv.Roles.Select(x =>
                                    x.RoleId
                                ).ToList(),
                        steps = dv is TaskVersion ? ((TaskVersion)dv).Steps.Select(s => new
                        {
                            s.Id,
                            s.Order,
                            s.Command,
                            s.TaskVersionId,
                        }) : null
                    }),
                    Roles = dt.DitatopicVersions.SelectMany(x => x.Roles).Select(x => new
                    {
                        x.RoleId,
                        x.Role.Name,
                    }).ToList(),
                }).ToList(),
                Documentos = f.Documentos.Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.DocFamilyId,
                    d.AuthorId,
                    d.FolderName,
                }).ToList()
            }).FirstOrDefaultAsync(x => x.Id == id);
            if (family == null)
            {
                return NotFound(new { message = "Document not found" });
            }
            return Ok(family);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFamily([FromBody] DocFamilyViewModel docFamily)
        {
            var family = new DocFamily
            {
                Title = docFamily.Title,
                Description = docFamily.Description,
            };
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            await _dbContext.DocFamilies.AddAsync(family);
            await _dbContext.SaveChangesAsync();
            family.FolderName = _ditaFileCreationService.ReplaceInvalidChars($"{family.Title}_id{family.Id}");

            _ditaFileCreationService.CreateFolderForDocument(family.FolderName);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return Ok(new { message = "Family is created successfully.", data = (GetFamiliyById(family.Id).GetAwaiter().GetResult() as OkObjectResult).Value });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFamily([FromRoute] int id, [FromBody] DocFamilyViewModel docFamily)
        {
            var family = await _dbContext.DocFamilies.Include(x => x.DitaTopics).Include(c => c.Documentos).FirstOrDefaultAsync(x => x.Id == id);
            if (family == null)
            {
                return NotFound(new { message = "Document not found" });
            }
            try
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                family.Title = docFamily.Title;
                family.Description = docFamily.Description;
                family.FolderName = _ditaFileCreationService.RenameFolder(oldFolderName: family.FolderName, newFolderName: $"{docFamily.Title}_id{docFamily.Id}"); ;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Document is updated successfully.", data = family });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFamily(int id)
        {
            var family = await _dbContext.DocFamilies.Include(x => x.DitaTopics).Include(c => c.Documentos).FirstOrDefaultAsync(x => x.Id == id);
            if (family == null)
            {
                return NotFound(new { message = "Family not found" });
            }
            _dbContext.DocFamilies.Remove(family);
            await _dbContext.SaveChangesAsync();
            _ditaFileCreationService.DeleteDocumentFolder(family.FolderName);
            return Ok(new { message = "Family is deleted successfully.", data = family.Id });
        }
    }
}