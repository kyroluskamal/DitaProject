using System.Diagnostics;
using AppConfgDocumentation.Data;
using AppConfgDocumentation.Models;
using AppConfgDocumentation.ModelViews;
using AppConfgDocumentation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppConfgDocumentation.enms;
using Microsoft.AspNetCore.Identity;

namespace AppConfgDocumentation.Controllers
{
    public class Documents : Controller_Base
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDitaFileCreationService _ditaFileCreationService;

        private readonly RoleManager<IdentityRole<int>> _roleManager;
        public Documents(ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment,
        IDitaFileCreationService ditaFileCreationService, RoleManager<IdentityRole<int>> roleManager)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _ditaFileCreationService = ditaFileCreationService;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments()
        {
            var documents = await _dbContext.Documentos.Include(f => f.Author).Include(x => x.DocVersions)
                .ThenInclude(c => c.Roles)
                .ThenInclude(c => c.Role)
                .Include(x => x.DocFamily)
                .ThenInclude(x => x.DitaTopics)
                .ThenInclude(c => c.DitatopicVersions)
                .ThenInclude(c => c.Roles)
                .ToListAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var document = await _dbContext.Documentos.Include(f => f.Author).Include(x => x.DocVersions)
             .ThenInclude(c => c.Roles)
                .ThenInclude(c => c.Role)
                .Include(x => x.DocFamily)
                .ThenInclude(x => x.DitaTopics)
                .ThenInclude(c => c.DitatopicVersions)
                .ThenInclude(c => c.Roles)
                .Select(x =>
                    new
                    {
                        x.Id,
                        x.Title,
                        x.Author,
                        DocVersions = x.DocVersions.Select(c => new DocVersion
                        {
                            Id = c.Id,
                            VersionNumber = c.VersionNumber,
                            DocumentId = c.DocumentId,
                            DitaTopics = c.DitatopicVersions.Select(x => x.DitatopicVersion).Select(y => y.DitaTopic).Distinct() as List<DitaTopic>,
                            DitatopicVersions = c.DitatopicVersions.Select(v => new DocVersionDitatopicVersion
                            {
                                DitatopicVersionId = v.DitatopicVersionId,
                                DocVersionId = v.DocVersionId,
                                DitatopicVersion = v.DitatopicVersion,
                            }).ToList(),
                            Roles = c.Roles.Select(r => new DocVersionsRoles
                            {
                                DocVersionId = r.DocVersionId,

                                RoleId = r.RoleId,
                                Role = r.Role
                            }).ToList()
                        }).ToList(),
                        DitaTopics = x.DocFamily.DitaTopics.Select(dt => new
                        {
                            dt.Id,
                            dt.Title,
                            dt.DocFamilyId,
                            dt.IsRequired,
                            Chosen = false,
                            DitatopicVersions = dt.DitatopicVersions.Select(dv => new
                            {
                                dv.Id,
                                dv.ShortDescription,
                                dv.FileName,
                                dv.VersionNumber,
                                dv.CreatedAt,
                                dv.DitaTopicId,
                                Type = dv is ConceptVersion ? 0 : dv is TaskVersion ? 1 : -1,
                                Body = dv is ConceptVersion ? ((ConceptVersion)dv).Body : null,
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
                        x.DocFamily
                    }
                )
                .FirstOrDefaultAsync(x => x.Id == id);
            if (document == null) return NotFound(new { message = "Document not found" });
            return Ok(document);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromBody] DucumentModelView document)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newDoc = new Documento { AuthorId = document.AuthorId, Title = document.Title, DocFamilyId = document.DocFamilyId };
            var family = await _dbContext.DocFamilies.FirstOrDefaultAsync(x => x.Id == document.DocFamilyId);
            if (family == null) return NotFound(new { message = "Document Family not found" });
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.Documentos.AddAsync(newDoc);
                await _dbContext.SaveChangesAsync();
                newDoc.FolderName = _ditaFileCreationService.ReplaceInvalidChars($"{newDoc.Title}_id{newDoc.Id}");
                var newVersion = new DocVersion { VersionNumber = document.VersionNumber, DocumentId = newDoc.Id };
                await _dbContext.DocVersions.AddAsync(newVersion);
                await _dbContext.SaveChangesAsync();
                var result = await addDocVersionsRoles(newVersion.Id);
                if (result > 0) _ditaFileCreationService.CreateFolderForDocument(Path.Combine(family.FolderName, newDoc.FolderName));
                await transaction.CommitAsync();

                return Ok(new { message = "Document is created successfully.", data = newDoc });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument([FromRoute] int id, [FromBody] DocumentUpdateViewModel documentViewModel)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.Include(v => v.DocFamily).Include(f => f.Author).Include(x => x.DocVersions)
                .ThenInclude(c => c.Roles)
                .ThenInclude(c => c.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                doc.Title = documentViewModel.Title;
                doc.FolderName = _ditaFileCreationService.RenameFolder(oldFolderName: Path.Combine(doc.DocFamily.FolderName, doc.FolderName), newFolderName: Path.Combine(doc.DocFamily.FolderName, $"{documentViewModel.Title}_id{doc.Id}"));

                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "Document is updated successfully.", data = doc });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync();
                Documento? doc = await _dbContext.Documentos.Include(x => x.DocVersions).ThenInclude(c => c.DitatopicVersions)
                .Include(x => x.DocVersions).ThenInclude(c => c.Roles).Include(x => x.DocVersions).ThenInclude(c => c.DitaTopics)
                .FirstOrDefaultAsync(x => x.Id == id);
                int docId = doc.Id;
                if (doc == null) return NotFound(new { message = "Document not found" });
                string folderName = doc.FolderName;

                List<DocVersionDitatopicVersion> ditatopicVersions = new();
                List<DocVersionsRoles> docVersionsRoles = new();
                foreach (var docVersion in doc.DocVersions)
                {
                    ditatopicVersions.AddRange(_dbContext.DocVersionDitatopicVersions.Where(x => x.DocVersionId == docVersion.Id));
                    docVersionsRoles.AddRange(_dbContext.DocVersionsRoles.Where(x => x.DocVersionId == docVersion.Id));
                }
                _dbContext.DocVersionDitatopicVersions.RemoveRange(ditatopicVersions);
                _dbContext.DocVersions.RemoveRange(doc.DocVersions);
                _dbContext.Documentos.Remove(doc);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                _ditaFileCreationService.DeleteDocumentFolder(folderName);

                return Ok(new { message = "Document is deleted successfully.", data = docId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //get version by id
        [HttpGet("{id}/versions/{versionId}")]
        public async Task<IActionResult> GetVersion([FromRoute] int id, [FromRoute] int versionId)
        {
            var docVersion = await _dbContext.DocVersions.Include(x => x.Document).Include(x => x.Roles)
            .Include(x => x.DitatopicVersions).FirstOrDefaultAsync(x => x.Id == versionId && x.DocumentId == id);
            if (docVersion == null) return NotFound(new { message = "Document version not found" });
            return Ok(docVersion);
        }

        //add version
        [HttpPost("{id}/versions")]
        public async Task<IActionResult> AddVersion([FromRoute] int id, [FromBody] DocVersionViewModel version)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                var newVersion = new DocVersion { VersionNumber = version.VersionNumber, DocumentId = doc.Id };
                await _dbContext.DocVersions.AddAsync(newVersion);
                await _dbContext.SaveChangesAsync();
                var result = await addDocVersionsRoles(newVersion.Id);
                transaction.Commit();
                if (result > 0)
                    return Ok(new { message = "Document Version is added successfully.", data = newVersion });
                return BadRequest(new { message = "Document Version is not added." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //get all versions of a document
        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetVersions([FromRoute] int id)
        {
            var docVersions = await _dbContext.DocVersions.Include(x => x.DitatopicVersions).ThenInclude(c => c.DitatopicVersion).Where(x => x.DocumentId == id).ToListAsync();
            return Ok(docVersions);
        }
        //update DocVersion
        [HttpPut("{id}/versions/{versionId}")]
        public async Task<IActionResult> UpdateVersion([FromRoute] int id, [FromRoute] int versionId, [FromBody] DocVersionUpdateViewModel version)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                DocVersion? docVersion = await _dbContext.DocVersions.Include(x => x.Document).Include(x => x.Roles)
            .Include(x => x.DitatopicVersions).FirstOrDefaultAsync(x => x.Id == versionId);
                if (docVersion == null) return NotFound(new { message = "Version not found" });

                docVersion.VersionNumber = version.VersionNumber;
                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "Document Version is updated successfully.", data = docVersion });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //delete DocVersion
        [HttpDelete("{id}/versions/{versionId}")]

        public async Task<IActionResult> DeleteVersion([FromRoute] int id, [FromRoute] int versionId)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                DocVersion? docVersion = await _dbContext.DocVersions.FirstOrDefaultAsync(x => x.Id == versionId);
                if (docVersion == null) return NotFound(new { message = "Version not found" });

                _dbContext.DocVersions.Remove(docVersion);
                var reslt = await _dbContext.SaveChangesAsync();
                if (reslt > 0)
                {
                    _ditaFileCreationService.DeleteDocumentFolder($"{_hostingEnvironment.WebRootPath}{doc.FolderName}\\{docVersion.VersionNumber}");
                }
                return Ok(new { message = "Document Version is deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //attach DitaTopicVersion to DocVersion
        [HttpPost("{id}/versions/{docVersionId}/ditaTopicVersions/{ditaTopicVersionId}")]
        public async Task<IActionResult> AttachDitaTopicVersion([FromRoute] int id, [FromRoute] int docVersionId, [FromRoute] int ditaTopicVersionId)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                DocVersion? docVersion = await _dbContext.DocVersions.FirstOrDefaultAsync(x => x.Id == docVersionId);
                if (docVersion == null) return NotFound(new { message = "Version not found" });

                DitatopicVersion? ditaTopicVersion = await _dbContext.DitatopicVersions.FirstOrDefaultAsync(x => x.Id == ditaTopicVersionId);
                if (ditaTopicVersion == null) return NotFound(new { message = "DitaTopicVersion not found" });

                var docVersionDitatopicVersion = new DocVersionDitatopicVersion { DocVersionId = docVersion.Id, DitatopicVersionId = ditaTopicVersion.Id };
                await _dbContext.DocVersionDitatopicVersions.AddAsync(docVersionDitatopicVersion);
                await _dbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetDocument), new { id = doc.Id }, doc);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //attach list of DitaTopicVersion to DocVersion
        [HttpPost("{id}/versions/{docVersionId}/ditaTopicVersions")]

        public async Task<IActionResult> AttachDitaTopicVersions([FromRoute] int id, [FromRoute] int docVersionId, [FromBody] List<int> ditaTopicVersionIds)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.Include(x => x.DocFamily)
                .ThenInclude(x => x.DitaTopics)
                .ThenInclude(x => x.DitatopicVersions).ThenInclude(x => x.Roles)
                .Include(x => x.DocVersions).ThenInclude(x => x.DitatopicVersions)
                .Include(x => x.DocVersions).ThenInclude(b => b.Roles).ThenInclude(r => r.Role).FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                DocVersion? docVersion = doc.DocVersions.FirstOrDefault(x => x.Id == docVersionId);
                if (docVersion == null) return NotFound(new { message = "Version not found" });

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                var ditaTopicVersions = await _dbContext.DocVersionDitatopicVersions
                .Where(x => x.DocVersionId == docVersion.Id)
                .ToListAsync();
                _dbContext.DocVersionDitatopicVersions.RemoveRange(ditaTopicVersions);
                await _dbContext.SaveChangesAsync();
                foreach (var ditaTopicVersionId in ditaTopicVersionIds)
                {
                    DitatopicVersion? ditaTopicVersion = await _dbContext.DitatopicVersions.FirstOrDefaultAsync(x => x.Id == ditaTopicVersionId);
                    if (ditaTopicVersion == null)
                        return NotFound(new { message = $"DitaTopicVersion number = {ditaTopicVersion?.VersionNumber} not found" });

                    var docVersionDitatopicVersion = new DocVersionDitatopicVersion { DocVersionId = docVersion.Id, DitatopicVersionId = ditaTopicVersion.Id };
                    await _dbContext.DocVersionDitatopicVersions.AddAsync(docVersionDitatopicVersion);
                }
                await _dbContext.SaveChangesAsync();

                await generateAllPDFsForAllRoles(docVersion, doc);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                var docs = GetDocument(id).GetAwaiter().GetResult() as OkObjectResult;
                return Ok(new { message = "DitaTopicVersions are attached successfully.", data = docs?.Value });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        //deattach DitaTopicVersion from DocVersion
        [HttpDelete("{id}/versions/{docVersionId}/ditaTopicVersions/{ditaTopicVersionId}")]
        public async Task<IActionResult> DeattachDitaTopicVersion([FromRoute] int id, [FromRoute] int docVersionId, [FromRoute] int ditaTopicVersionId)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                DocVersion? docVersion = await _dbContext.DocVersions.FirstOrDefaultAsync(x => x.Id == docVersionId);
                if (docVersion == null) return NotFound(new { message = "Version not found" });

                DitatopicVersion? ditaTopicVersion = await _dbContext.DitatopicVersions.FirstOrDefaultAsync(x => x.Id == ditaTopicVersionId);
                if (ditaTopicVersion == null) return NotFound(new { message = "DitaTopicVersion not found" });

                var docVersionDitatopicVersion = await _dbContext.DocVersionDitatopicVersions.FirstOrDefaultAsync(x => x.DocVersionId == docVersion.Id && x.DitatopicVersionId == ditaTopicVersion.Id);
                if (docVersionDitatopicVersion == null) return NotFound(new { message = "DitaTopicVersion not attached to DocVersion" });

                _dbContext.DocVersionDitatopicVersions.Remove(docVersionDitatopicVersion);
                await _dbContext.SaveChangesAsync();
                return Ok("DitaTopicVersion is deattached successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private string generateDitaMapXml(List<DitatopicVersion> ditatopicVersions, Documento document, DocVersion docVersion)
        {
            var topicRefs = ditatopicVersions.Select(x => $"<topicref href='../{_ditaFileCreationService.ReplaceInvalidChars(x.FileName)}.dita' />");

            var ditaMapXml = $@"<?xml version='1.0' encoding='UTF-8'?>
                <!DOCTYPE map PUBLIC '-//OASIS//DTD DITA Map//EN' '{_hostingEnvironment.WebRootPath.Replace("\\", "/")}/dtd/map.dtd'>
                <map>
                    <title>{document.Title} - V{docVersion.VersionNumber}</title>
                    {string.Join("", topicRefs)}
                </map>";
            return ditaMapXml;
        }
        private async Task<ReturnTypeOfDitaToics> generateAllPDFsForAllRoles(DocVersion docVersion, Documento doc)
        {
            var res = new ReturnTypeOfDitaToics();
            try
            {
                foreach (var role in docVersion.Roles)
                {
                    var ditatopicversionsByRole = doc.DocFamily.DitaTopics.SelectMany(x => x.DitatopicVersions).Where(x => x.Roles.Any(r => r.RoleId == role.RoleId)).ToList();
                    role.DitaMapXml = generateDitaMapXml(ditatopicversionsByRole, doc, docVersion);
                    role.DitaMapFilePath = _ditaFileCreationService.SaveDitaFile(role.DitaMapXml, Path.Combine(doc.DocFamily.FolderName, doc.FolderName), docVersion.VersionNumber,
                    DitaFileExtensions.ditamap, role.Role.Name);
                    res = await GeneratePDF(request: new GeneratePDFModelView(docV: docVersion, docVersionRole: role));
                }
                return res;
            }
            catch (Exception ex)
            {
                res = new ReturnTypeOfDitaToics { Code = 0, Message = ex.Message };
                return res;
            }
        }
        private async Task<int> addDocVersionsRoles(int versionId)
        {
            var allroles = await _roleManager.Roles.ToListAsync();
            var docVersionRoles = new List<DocVersionsRoles>();
            foreach (var role in allroles)
            {
                var docVRole = new DocVersionsRoles { DocVersionId = versionId, RoleId = role.Id };
                docVersionRoles.Add(docVRole);
            }
            await _dbContext.DocVersionsRoles.AddRangeAsync(docVersionRoles);
            var result = await _dbContext.SaveChangesAsync();

            return result;
        }
        private async Task<ReturnTypeOfDitaToics> GeneratePDF(GeneratePDFModelView request)
        {
            var res = new ReturnTypeOfDitaToics();
            try
            {
                var outputPdfPath = Path.Combine(_hostingEnvironment.WebRootPath, request.docV.Document.DocFamily.FolderName, request.docV.Document.FolderName
                , $"{request.docV.VersionNumber}");
                var finalOutputPdfPath = $"{outputPdfPath}\\{request.docV.VersionNumber}{request.docVersionRole.Role.Name}.pdf";
                var relativePath = Path.GetRelativePath(_hostingEnvironment.WebRootPath, finalOutputPdfPath);
                if (System.IO.File.Exists(finalOutputPdfPath))
                {
                    System.IO.File.Delete(finalOutputPdfPath);
                }
                var ditaOtCommand = @"C:\dita\bin\dita.bat";
                var args = $"--input=\"{request.docVersionRole.DitaMapFilePath}\" --output=\"{outputPdfPath}\" --format=pdf";
                args = args.Replace("\\", "/");
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = ditaOtCommand,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();
                // To read the output (stdout)
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                if (!string.IsNullOrEmpty(error))
                {
                    res = new ReturnTypeOfDitaToics { Code = 0, Message = error };
                    return res;
                }
                outputPdfPath = $"{outputPdfPath}\\{request.docV.VersionNumber}.pdf";
                // Further check if the PDF was generated successfully.
                request.docVersionRole.PDFfilePath = relativePath;
                if (!System.IO.File.Exists(outputPdfPath))
                {
                    res = new ReturnTypeOfDitaToics { Code = 0, Message = "PDF generation failed." };
                    return res;
                }
                await _dbContext.SaveChangesAsync();
                return new ReturnTypeOfDitaToics { Code = 1, Message = "PDF generated successfully.", data = relativePath };
            }
            catch (Exception ex)
            {
                res = new ReturnTypeOfDitaToics { Code = 0, Message = ex.Message };
                return res;
            }
        }
    }
}