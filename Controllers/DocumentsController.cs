using System;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using AppConfgDocumentation.Data;
using AppConfgDocumentation.Models;
using AppConfgDocumentation.ModelViews;
using AppConfgDocumentation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using AppConfgDocumentation.enms;
using Microsoft.AspNetCore.Identity.Data;

namespace AppConfgDocumentation.Controllers
{
    public class Documents : Controller_Base
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDitaFileCreationService _ditaFileCreationService;
        public Documents(ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment,
        IDitaFileCreationService ditaFileCreationService)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _ditaFileCreationService = ditaFileCreationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments()
        {
            var documents = await _dbContext.Documentos.Include(f => f.Author).Include(x => x.DocVersions)
                .Include(x => x.DitaTopics)
                .ThenInclude(c => c.DitatopicVersions)
                .ToListAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var document = await _dbContext.Documentos.Include(f => f.Author).Include(x => x.DocVersions)
                .Include(x => x.DitaTopics)
                .ThenInclude(c => c.DitatopicVersions)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (document == null) return NotFound(new { message = "Document not found" });
            return Ok(document);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromBody] DucumentModelView document)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newDoc = new Documento { AuthorId = document.AuthorId, Title = document.Title };
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.Documentos.AddAsync(newDoc);
                await _dbContext.SaveChangesAsync();
                newDoc.FolderName = $"{document.Title}_id{newDoc.Id}";
                var newVersion = new DocVersion { VersionNumber = document.VersionNumber, DocumentId = newDoc.Id };
                await _dbContext.DocVersions.AddAsync(newVersion);
                await _dbContext.SaveChangesAsync();

                _ditaFileCreationService.CreateFolderForDocument(newDoc.FolderName);
                await transaction.CommitAsync();

                return Ok(new { message = "Document is created successfully.", data = newDoc });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument([FromRoute] int id, [FromBody] string title)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                doc.Title = title;
                doc.FolderName = _ditaFileCreationService.RenameFolder(oldFileName: doc.FolderName, newFolderName: $"{title}_id{doc.Id}");

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
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });
                string folderName = doc.FolderName;
                _dbContext.Documentos.Remove(doc);
                _ditaFileCreationService.DeleteDocumentFolder(folderName);
                await _dbContext.SaveChangesAsync();
                return Ok("Document is deleted successfully.");
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
            var docVersion = await _dbContext.DocVersions.Include(x => x.Document).FirstOrDefaultAsync(x => x.Id == versionId && x.DocumentId == id);
            if (docVersion == null) return NotFound(new { message = "Document version not found" });
            return Ok(docVersion);
        }

        //add version
        [HttpPost("{id}/versions")]
        public async Task<IActionResult> AddVersion([FromRoute] int id, [FromBody] DocVersionViewModel version)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                var newVersion = new DocVersion { VersionNumber = version.VersionNumber, DocumentId = doc.Id };
                await _dbContext.DocVersions.AddAsync(newVersion);
                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "Document Version is added successfully.", data = newVersion });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //update DocVersion
        [HttpPut("{id}/versions/{versionId}")]
        public async Task<IActionResult> UpdateVersion([FromRoute] int id, [FromRoute] int versionId, [FromBody] DocVersionViewModel version)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                DocVersion? docVersion = await _dbContext.DocVersions.FirstOrDefaultAsync(x => x.Id == versionId);
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
                await _dbContext.SaveChangesAsync();
                return Ok("Document Version is deleted successfully.");
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
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });

                DocVersion? docVersion = await _dbContext.DocVersions.FirstOrDefaultAsync(x => x.Id == docVersionId);
                if (docVersion == null) return NotFound(new { message = "Version not found" });

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                foreach (var ditaTopicVersionId in ditaTopicVersionIds)
                {
                    DitatopicVersion? ditaTopicVersion = await _dbContext.DitatopicVersions.FirstOrDefaultAsync(x => x.Id == ditaTopicVersionId);
                    if (ditaTopicVersion == null)
                        return NotFound(new { message = $"DitaTopicVersion number = {ditaTopicVersion?.VersionNumber} not found" });

                    var docVersionDitatopicVersion = new DocVersionDitatopicVersion { DocVersionId = docVersion.Id, DitatopicVersionId = ditaTopicVersion.Id };
                    await _dbContext.DocVersionDitatopicVersions.AddAsync(docVersionDitatopicVersion);
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return CreatedAtAction(nameof(GetDocument), new { id = doc.Id }, doc);
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


        // create ditamap for doc version
        [HttpPut("{id}/versions/{docVersionId}/ditamap")]

        public async Task<IActionResult> CreateDitamap([FromRoute] int id, [FromRoute] int docVersionId)
        {
            try
            {
                Documento? doc = await _dbContext.Documentos.FirstOrDefaultAsync(x => x.Id == id);
                if (doc == null) return NotFound(new { message = "Document not found" });


                DocVersion? docVersion = await _dbContext.DocVersions.FirstOrDefaultAsync(x => x.Id == docVersionId);
                if (docVersion == null) return NotFound(new { message = "Version not found" });

                docVersion.DitaMapXml = generateDitaMapXml(docVersion, doc);

                docVersion.DitaMapFilePath = _ditaFileCreationService.SaveDitaFile(docVersion.DitaMapXml, doc.FolderName, docVersion.VersionNumber,
                DitaFileExtensions.ditamap);
                await _dbContext.SaveChangesAsync();
                return Ok(docVersion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }


        [HttpPost("{id}/versions/{docVersionId}/ditamap")]
        public async Task<IActionResult> GeneratePDF([FromRoute] int id, [FromRoute] int docVersionId)
        {
            var docV = await _dbContext.DocVersions.Include(x => x.Document).FirstOrDefaultAsync(x => x.Id == docVersionId && x.DocumentId == id);
            if (docV == null) return NotFound(new { message = "Document not found" });
            try
            {
                var outputPdfPath = Path.Combine(_hostingEnvironment.WebRootPath, docV.Document.FolderName
                , $"{docV.VersionNumber}");
                var finalOutputPdfPath = $"{outputPdfPath}\\{docV.VersionNumber}.pdf";
                var relativePath = Path.GetRelativePath(_hostingEnvironment.WebRootPath, finalOutputPdfPath);
                if (System.IO.File.Exists(finalOutputPdfPath))
                    return Ok(new { message = "PDF already generated.", pdfPath = relativePath });
                Debug.WriteLine("outputPdfPath", outputPdfPath);
                var ditaOtCommand = @"C:\dita\bin\dita.bat"; ;
                var args = $"--input=\"{docV.DitaMapFilePath}\" --output=\"{outputPdfPath}\" --format=pdf";
                args = args.Replace("\\", "/");
                Debug.WriteLine("args", args);
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
                string result = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    // Log the error.
                    Console.WriteLine(error);
                    return BadRequest(new { message = error });
                }
                outputPdfPath = $"{outputPdfPath}\\{docV.VersionNumber}.pdf";
                // Further check if the PDF was generated successfully.
                if (!System.IO.File.Exists(outputPdfPath))
                {
                    return BadRequest(new { message = "PDF generation failed." });
                }
                docV.PDFfilePath = relativePath;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "PDF generated successfully.", pdfPath = relativePath, result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        private string generateDitaMapXml(DocVersion docVersion, Documento document)
        {

            var attachedDitaTopicVersions = _dbContext.DocVersionDitatopicVersions
                .Include(x => x.DitatopicVersion)
                .Where(x => x.DocVersionId == docVersion.Id)
                .Select(x => x.DitatopicVersion)
                .ToList();
            var topicRefs = attachedDitaTopicVersions.Select(x => $"<topicref href='{_ditaFileCreationService.ReplaceInvalidChars(x.FileName)}.dita' />");

            var ditaMapXml = $@"<?xml version='1.0' encoding='UTF-8'?>
                <!DOCTYPE map PUBLIC '-//OASIS//DTD DITA Map//EN' '{_hostingEnvironment.WebRootPath.Replace("\\", "/")}/dtd/map.dtd'>
                <map>
                    <title>{document.Title} - V{docVersion.VersionNumber}</title>
                    {string.Join("", topicRefs)}
                </map>";
            return ditaMapXml;
        }
    }

}