using System.ComponentModel.DataAnnotations;
using AppConfgDocumentation.Data;
using AppConfgDocumentation.enms;
using AppConfgDocumentation.Models;
using AppConfgDocumentation.ModelViews;
using AppConfgDocumentation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppConfgDocumentation.Controllers
{
    class DitaTopicFileName
    {
        public string Title { get; set; }
        public int topicId { get; set; }
        public int versionId { get; set; }
        public string versionNumber { get; set; }
    }
    delegate Task<ReturnTypeOfDitaToics> GetDitaObject(DitaTopicModelView topic);
    public class DitaTopicsController : Controller_Base
    {
        //add dbcontext
        private readonly ApplicationDbContext _context;
        private readonly IDitaFileCreationService _ditaFileCreationService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DitaTopicsController(ApplicationDbContext context, IDitaFileCreationService ditaFileCreationService, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _ditaFileCreationService = ditaFileCreationService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDitaTopics()
        {
            return Ok(await _context.DitaTopics
                        .Include(x => x.DitatopicVersions)
                        .Select(dt => new
                        {
                            dt.Id,
                            dt.Title,
                            dt.CreatedAt,
                            dt.DocumentId,
                            Type = dt is Concept ? 0 : dt is Tasks ? 1 : -1,
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
                                steps = dv is TaskVersion ? ((TaskVersion)dv).Steps.Select(s => new
                                {
                                    s.Id,
                                    s.Order,
                                    s.Command,
                                    s.TaskVersionId,
                                }) : null
                            })
                        })
                        .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDitaTopicById(int id)
        {
            var ditaTopic = await _context.DitaTopics
                        .Include(x => x.Document)
                        .Include(x => x.DitatopicVersions)
                        .Select(dt => new
                        {
                            dt.Id,
                            dt.Title,
                            dt.CreatedAt,
                            dt.DocumentId,
                            Type = dt is Concept ? 0 : dt is Tasks ? 1 : -1,
                            DitatopicVersions = dt.DitatopicVersions.Select(dv => new
                            {
                                dv.Id,
                                dv.ShortDescription,
                                dv.FileName,
                                dv.VersionNumber,
                                dv.CreatedAt,
                                dv.XmlContent,
                                dv.DitaTopicId,
                                Type = dv is ConceptVersion ? 0 : dv is TaskVersion ? 1 : -1,
                                body = dv is ConceptVersion ? ((ConceptVersion)dv).Body : null,
                                steps = dv is TaskVersion ? ((TaskVersion)dv).Steps.Select(s => new
                                {
                                    s.Id,
                                    s.Order,
                                    s.Command,
                                    s.TaskVersionId
                                }) : null
                            })
                        }).FirstOrDefaultAsync(x => x.Id == id);
            if (ditaTopic == null)
                return NotFound(new { message = $"DITA topic with ID {id} not found" });
            return Ok(ditaTopic);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DitaTopicModelView topic)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            ReturnTypeOfDitaToics result = new();
            if (topic.Type == (int)DitaTopicTypes.Concept)
                result = await AddUpdateDitaTopicAndVersions(topic, AddConcept);
            else
                result = await AddUpdateDitaTopicAndVersions(topic, AddTask);

            return result.Code == 0 ? BadRequest(new { message = result.Message }) :
                Ok(new { message = result.Message, data = ((OkObjectResult)GetDitaTopicById(result.Ditatopic.Id).GetAwaiter().GetResult()).Value });
        }

        // PUT: api/DitaTopics/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] DitaTopicUpdateViewModel topic)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newTopic = new DitaTopicModelView
            {
                Id = id,
                Title = topic.Title,
                DocumentId = topic.DocumentId,
                Type = topic.Type
            };
            var result = await AddUpdateDitaTopicAndVersions(newTopic, UpdateDitaTopic, isUpdate: true);

            return result.Code == 0 ? BadRequest(new { message = result.Message })
                    : Ok(new { message = result.Message, data = ((OkObjectResult)GetDitaTopicById(result.Ditatopic.Id).GetAwaiter().GetResult()).Value });
        }

        // DELETE: api/DitaTopics/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] int docId)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                var ditaTopic = await _context.DitaTopics.Include(c => c.DitatopicVersions).ThenInclude(x => x.DocVersions).FirstOrDefaultAsync(x => x.Id == id);
                if (ditaTopic == null)
                    return NotFound(new { message = $"DITA topic with ID {id} not found" });
                _context.DitaTopics.Remove(ditaTopic);
                await _context.SaveChangesAsync();
                foreach (var version in ditaTopic.DitatopicVersions)
                {
                    System.IO.File.Delete(version.FilePath);
                }
                await transaction.CommitAsync();
                return Ok(new { message = $"DITA topic with ID {id} deleted Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //add Version to DitaTopic
        [HttpPost("version")]

        public async Task<IActionResult> AddVersion([FromBody] DitaTopicVersionViewModel ditaTopicVersion)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = new ReturnTypeOfDitaToics();

            var ditaTopicViewModel = new DitaTopicModelView
            {
                Id = ditaTopicVersion.DitaTopicId,
                VersionId = ditaTopicVersion.Id,
                // DitaTopicId = ditaTopicVersion.DitaTopicId,
                ShortDescription = ditaTopicVersion.ShortDescription,
                VersionNumber = ditaTopicVersion.VersionNumber,
                Type = ditaTopicVersion.Type,
                Body = ditaTopicVersion.Body,
                Steps = ditaTopicVersion.Steps
            };
            result = await AddUpdateDitaTopicAndVersions(ditaTopicViewModel, AddDitaTopicVersion);

            return result.Code == 0 ? BadRequest(new { message = result.Message }) :
                Ok(new { message = result.Message, data = result.DitatopicVersion });
        }

        // PUT: api/DitaTopics/version/{id}
        [HttpPut("version/{id}")]
        public async Task<IActionResult> UpdateVersion([FromRoute] int id, [FromBody] DitaTopicVersionViewModel ditaTopicVersion)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var ditaTopicViewModel = new DitaTopicModelView
            {
                Id = ditaTopicVersion.DitaTopicId,
                VersionId = id,
                // DitaTopicId = ditaTopicVersion.DitaTopicId,
                ShortDescription = ditaTopicVersion.ShortDescription,
                VersionNumber = ditaTopicVersion.VersionNumber,
                Type = ditaTopicVersion.Type,
                Body = ditaTopicVersion.Body,
                Steps = ditaTopicVersion.Steps
            }; var result = await AddUpdateDitaTopicAndVersions(ditaTopicViewModel, UpdateDitaTopicVersion, isUpdate: true);

            return result.Code == 0 ? BadRequest(new { message = result.Message }) :
                Ok(new { message = result.Message, data = result.DitatopicVersion });
        }

        //Delete Version
        [HttpDelete("version/{id}")]
        public async Task<IActionResult> DeleteVersion([FromRoute] int id)
        {
            try
            {
                var version = await _context.DitatopicVersions.FirstOrDefaultAsync(x => x.Id == id);
                if (version == null)
                    return NotFound(new { message = $"DITA topic version with ID {id} not found" });
                var FilePath = version.FilePath;
                _context.DitatopicVersions.Remove(version);
                var saveChangesResult = await _context.SaveChangesAsync();
                System.IO.File.Delete(FilePath);

                if (saveChangesResult == 0)
                {
                    return BadRequest(new { message = "No changes were made to the database." });
                }
                return Ok(new { message = $"DITA topic version with ID {id} deleted Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        private async Task<ReturnTypeOfDitaToics> AddUpdateDitaTopicAndVersions(DitaTopicModelView topic, GetDitaObject ditaObject, bool isUpdate = false)
        {
            var result = new ReturnTypeOfDitaToics();
            try
            {
                var doc = await _context.Documentos.Include(x => x.DitaTopics).FirstOrDefaultAsync(x => x.Id == topic.DocumentId);
                using var transaction = await _context.Database.BeginTransactionAsync();

                result = await ditaObject(topic);
                if (result.DitatopicVersion != null)
                {
                    if (isUpdate) System.IO.File.Delete(result.DitatopicVersion.FilePath);
                    UpdateFileNameAndPathInDitatopicVersion(result.DitatopicVersion, result.Ditatopic!, (string)(doc != null ? doc?.FolderName! : result.Ditatopic!.Document?.FolderName!));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.Message = "Dita concept created successfully";
                return result;
            }
            catch (Exception ex)
            {
                result.Code = 0;
                result.Message = ex.Message;
                return result;
            }
        }

        private async Task<ReturnTypeOfDitaToics> AddConcept(DitaTopicModelView topic)
        {
            var concept = new Concept
            {
                Title = topic.Title,
                DocumentId = topic.DocumentId
            };
            var version = new ConceptVersion
            {
                VersionNumber = topic.VersionNumber,
                ShortDescription = topic.ShortDescription,
                Body = topic.Body
            };
            var result = await AddDitaTopic(concept, version);

            if (result.Code == 0)
                result.Message = "Failed to create Dita concept";
            else
            {
                result.Message = "Dita concept created successfully";
                result.Ditatopic = ((OkObjectResult)GetDitaTopicById(concept.Id).GetAwaiter().GetResult()).Value;
            }
            return result;
        }

        private async Task<ReturnTypeOfDitaToics> AddTask(DitaTopicModelView topic)
        {
            var task = new Tasks
            {
                Title = topic.Title,
                DocumentId = topic.DocumentId
            };
            var version = new TaskVersion
            {
                ShortDescription = topic.ShortDescription,
                VersionNumber = topic.VersionNumber
            };
            foreach (var step in topic.Steps)
                version.Steps.Add(new Step { Order = step.Order, Command = step.Command });

            var result = await AddDitaTopic(task, version);
            if (result.Code == 0)
                result.Message = "Failed to create Dita task";
            else result.Message = "Dita task created successfully";

            return result;
        }

        private async Task<ReturnTypeOfDitaToics> UpdateDitaTopic(DitaTopicModelView topic)
        {
            try
            {
                var result = new ReturnTypeOfDitaToics();
                var ditaTopic = await _context.DitaTopics.Include(x => x.Document)
                .Include(x => x.DitatopicVersions).FirstOrDefaultAsync(x => x.Id == topic.Id && x.DocumentId == topic.DocumentId);

                if (ditaTopic == null)
                {
                    result.Code = 0;
                    result.Message = "Dita topic not found";
                    return result;
                }

                ditaTopic.Title = topic.Title;
                var directory =
                result.Code = await _context.SaveChangesAsync();

                foreach (var version in ditaTopic.DitatopicVersions)
                {
                    var versiosnPath = $"{_hostingEnvironment.WebRootPath}\\{ditaTopic.Document?.FolderName}\\{_ditaFileCreationService.ReplaceInvalidChars(version.FileName)}.dita";
                    System.IO.File.Delete(versiosnPath);
                    if (version is TaskVersion)
                    {
                        (version as TaskVersion).Steps = _context.Steps.Where(x => x.TaskVersionId == version.Id).ToList();
                    }
                    UpdateFileNameAndPathInDitatopicVersion(version, ditaTopic, ditaTopic.Document?.FolderName!);
                }

                result.Ditatopic = ditaTopic;
                result.DitatopicVersion = null;
                if (result.Code == 0)
                {
                    result.Message = "Failed to update Dita topic";
                    return result;
                }
                result.Message = "Dita topic updated successfully";
                return result;
            }
            catch (Exception ex)
            {
                return new ReturnTypeOfDitaToics { Code = 0, Message = ex.Message };
            }
        }
        private async Task<ReturnTypeOfDitaToics> AddDitaTopicVersion(DitaTopicModelView ditaTopicModelView)
        {
            var result = new ReturnTypeOfDitaToics();
            var ditaTopic = await _context.DitaTopics.Include(x => x.Document).FirstOrDefaultAsync(x => x.Id == ditaTopicModelView.Id);

            if (ditaTopic == null)
            {
                result.Code = 0;
                result.Message = "Dita topic not found";
                return result;
            }
            DitatopicVersion? version = null;

            if (ditaTopicModelView.Type == (int)DitaTopicTypes.Concept)
            {
                version = new ConceptVersion
                {
                    ShortDescription = ditaTopicModelView.ShortDescription,
                    VersionNumber = ditaTopicModelView.VersionNumber,
                    DitaTopicId = ditaTopicModelView.Id,
                    Body = ditaTopicModelView.Body
                };

            }
            else
            {
                var taskVersion = new TaskVersion
                {
                    ShortDescription = ditaTopicModelView.ShortDescription,
                    VersionNumber = ditaTopicModelView.VersionNumber,
                    DitaTopicId = ditaTopicModelView.Id,
                    Steps = ditaTopicModelView.Steps.Select(x => new Step
                    {
                        Order = x.Order,
                        Command = x.Command
                    }).ToList()
                };

                version = taskVersion;
            }
            await _context.DitatopicVersions.AddAsync(version);

            result.Code = await _context.SaveChangesAsync();
            result.Message = "Dita topic version added successfully";
            result.DitatopicVersion = version;
            result.Ditatopic = ditaTopic;
            return result;
        }

        private async Task<ReturnTypeOfDitaToics> UpdateDitaTopicVersion(DitaTopicModelView ditaTopicModelView)
        {
            var result = new ReturnTypeOfDitaToics();
            var version = await _context.DitatopicVersions.
            Include(x => x.DitaTopic)
            .ThenInclude(x => x.Document)
            .FirstOrDefaultAsync(x => x.Id == ditaTopicModelView.VersionId);
            if (version == null)
            {
                result.Code = 0;
                result.Message = "Dita topic version not found";
                return result;
            }
            version.ShortDescription = ditaTopicModelView.ShortDescription;
            version.VersionNumber = ditaTopicModelView.VersionNumber;

            if (version is ConceptVersion conceptVersion)
            {
                conceptVersion.Body = ditaTopicModelView.Body;
            }
            else if (version is TaskVersion taskVersion)
            {
                taskVersion.Steps.Clear();
                foreach (var step in ditaTopicModelView.Steps)
                {
                    taskVersion.Steps.Add(new Step
                    {
                        Order = step.Order,
                        Command = step.Command
                    });
                }
            }
            result.Code = await _context.SaveChangesAsync();
            result.Message = "Dita topic version updated successfully";
            result.DitatopicVersion = version;
            result.Ditatopic = version.DitaTopic;
            return result;
        }

        private async Task<ReturnTypeOfDitaToics> AddDitaTopic(DitaTopic topic, DitatopicVersion version)
        {
            await _context.DitaTopics.AddAsync(topic);
            await _context.SaveChangesAsync();
            version.DitaTopicId = topic.Id;
            await _context.DitatopicVersions.AddAsync(version);
            var result = await _context.SaveChangesAsync();

            var topicInDb = ((OkObjectResult)GetDitaTopicById(topic.Id).GetAwaiter().GetResult()).Value;

            return new ReturnTypeOfDitaToics { Code = result, Ditatopic = topicInDb, DitatopicVersion = version };
        }


        private void UpdateFileNameAndPathInDitatopicVersion(DitatopicVersion version, dynamic ditaTopic, dynamic? docFolderName)
        {
            version.FileName = $@"{ditaTopic.Title}_T_id_{ditaTopic.Id}_V_id_{version.Id}_v_no_{version.VersionNumber}";
            version.XmlContent = GenerateXml(ditaTopic.Title.Replace(" ", "") + version.VersionNumber,
            ditaTopic.Title, version.ShortDescription,
             version is ConceptVersion ? ((ConceptVersion)version).Body : generateTaskStepsXml(((TaskVersion)version).Steps),
             version is ConceptVersion ? "concept" : "task");
            version.FilePath = _ditaFileCreationService.SaveDitaFile(version.XmlContent, docFolderName ?? "", version.FileName);
        }

        private string GenerateXml(string id, string title, string shortdesc, string body, string type = "concept")
        {
            var shorsDesc = string.IsNullOrEmpty(shortdesc) ? "" : $"<shortdesc>{shortdesc}</shortdesc>";
            return $@"<?xml version='1.0' encoding='UTF-8'?>
                <!DOCTYPE {type} PUBLIC '-//OASIS//DTD DITA {char.ToUpper(type[0]) + type[1..]}//EN' '../dtd/{type}.dtd'>
                <{type} id='{id}'>
                    <title>{title}</title>
                    {shorsDesc}
                    <{(type == "concept" ? "con" : "task")}body>
                        {body}
                    </{(type == "concept" ? "con" : "task")}body>
                </{type}>
            ";
        }

        private string generateTaskStepsXml(ICollection<Step> steps)
        {
            string stepsXml = "<steps>";
            foreach (var step in steps)
            {
                stepsXml += $@"<step>
                    <cmd>{step.Command}</cmd>
                </step>";
            }
            stepsXml += "</steps>";
            return stepsXml;
        }
    }
}