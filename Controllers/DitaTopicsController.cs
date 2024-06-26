using System.ComponentModel.DataAnnotations;
using System.Text;
using AppConfgDocumentation.Data;
using AppConfgDocumentation.enms;
using AppConfgDocumentation.Models;
using AppConfgDocumentation.ModelViews;
using AppConfgDocumentation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppConfgDocumentation.Controllers
{
    // class DitaTopicFileName
    // {
    //     public string Title { get; set; } = string.Empty;
    //     public int topicId { get; set; }
    //     public int versionId { get; set; }
    //     public string versionNumber { get; set; } = string.Empty;
    // }
    delegate Task<ReturnTypeOfDitaToics> GetDitaObject(DitaTopicVersionViewModel topic);
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
                        .ThenInclude(x => x.Roles)
                        .Select(dt => new
                        {
                            dt.Id,
                            dt.Title,
                            dt.CreatedAt,
                            dt.DocFamilyId,
                            dt.IsRequired,
                            // Type = dt is Concept ? 0 : dt is Tasks ? 1 : -1,
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
                            }),
                            Roles = dt.DitatopicVersions.SelectMany(x => x.Roles).Select(x => new
                            {
                                x.RoleId,
                                x.Role.Name,
                            })
                        })
                        .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDitaTopicById(int id)
        {
            var ditaTopic = await _context.DitaTopics
                        .Include(x => x.DitatopicVersions)
                        .ThenInclude(x => x.Roles)
                        .Select(dt => new
                        {
                            dt.Id,
                            dt.Title,
                            dt.CreatedAt,
                            dt.DocFamilyId,
                            dt.IsRequired,
                            // Type = dt is Concept ? 0 : dt is Tasks ? 1 : -1,
                            DitatopicVersions = dt.DitatopicVersions.Select(dv => new
                            {
                                dv.Id,
                                dv.ShortDescription,
                                dv.FileName,
                                dv.VersionNumber,
                                dv.CreatedAt,
                                dv.DitaTopicId,
                                Roles = dv.Roles.Select(x =>
                                    x.RoleId
                                ).ToList(),
                                Type = dv is ConceptVersion ? 0 : dv is TaskVersion ? 1 : -1,
                                body = dv is ConceptVersion ? ((ConceptVersion)dv).Body : null,
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
                            }),

                        }).FirstOrDefaultAsync(x => x.Id == id);
            if (ditaTopic == null)
                return NotFound(new { message = $"DITA topic with ID {id} not found" });
            return Ok(ditaTopic);
        }

        [HttpPost]
        private async Task<IActionResult> PostOne(DitaTopicModelView topic)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dt = new DitaTopic { Title = topic.Title, DocFamilyId = topic.DocFamilyId, IsRequired = topic.IsRequired };
            try
            {
                await _context.DitaTopics.AddAsync(dt);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Dita topic is added successfully", data = ((OkObjectResult)GetDitaTopicById(dt.Id).GetAwaiter().GetResult()).Value });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<DitaTopicModelView> topics)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var dts = new List<DitaTopic>();
            foreach (var topic in topics)
            {
                dts.Add(new DitaTopic { Title = topic.Title, DocFamilyId = topic.DocFamilyId, IsRequired = topic.IsRequired });
            }
            try
            {
                await _context.DitaTopics.AddRangeAsync(dts);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Dita topic is added successfully", data = dts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // public async Task<IActionResult> Post([FromBody] DitaTopicModelView topic)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);
        //     ReturnTypeOfDitaToics result = new();
        //     if (topic.Type == (int)DitaTopicTypes.Concept)
        //         result = await AddUpdateDitaTopicAndVersions(topic, AddConcept);
        //     else
        //         result = await AddUpdateDitaTopicAndVersions(topic, AddTask);

        //     return result.Code == 0 ? BadRequest(new { message = result.Message }) :
        //         Ok(new { message = result.Message, data = ((OkObjectResult)GetDitaTopicById(result.Ditatopic?.Id).GetAwaiter().GetResult()).Value });
        // }

        // PUT: api/DitaTopics/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] DitaTopicModelView topic)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var ditaTopic = await _context.DitaTopics.Include(x => x.DocFamily).FirstOrDefaultAsync(x => x.Id == id);
                if (ditaTopic == null)
                    return NotFound(new { message = $"DITA topic with ID {id} not found" });

                ditaTopic.Title = topic.Title;
                ditaTopic.DocFamilyId = topic.DocFamilyId;
                ditaTopic.IsRequired = topic.IsRequired;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Dita topic is updated Successfylly ", data = ((OkObjectResult)GetDitaTopicById(ditaTopic.Id).GetAwaiter().GetResult()).Value });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] List<DitaTopicModelView> topics)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                foreach (var topic in topics)
                {
                    var ditaTopic = await _context.DitaTopics.FirstOrDefaultAsync(x => x.Id == topic.Id);
                    if (ditaTopic == null)
                        await PostOne(topic);
                    else
                    {
                        ditaTopic.Title = topic.Title;
                        ditaTopic.DocFamilyId = topic.DocFamilyId;
                        ditaTopic.IsRequired = topic.IsRequired;
                        ditaTopic.IsRequired = topic.IsRequired;
                        await _context.SaveChangesAsync();
                    }
                }
                await transaction.CommitAsync();
                var ditaTopics = await _context.DitaTopics.Include(x => x.DocFamily).Where(x => x.DocFamilyId == topics[0].DocFamilyId).ToListAsync();
                return Ok(new { message = "Dita topic is updated Successfylly ", data = ditaTopics });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/DitaTopics/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
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

        //get DitaTopicVersion by Id
        [HttpGet("version/{id}")]
        public async Task<IActionResult> GetDitaTopicVersionById(int id)
        {
            var version = await _context.DitatopicVersions
                        .Include(x => x.DitaTopic)
                        .ThenInclude(x => x.DocFamily)
                        .Select(dv => new
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
                        }).FirstOrDefaultAsync(x => x.Id == id);
            if (version == null)
                return NotFound(new { message = $"DITA topic version with ID {id} not found" });
            return Ok(version);
        }

        //add Version to DitaTopic
        [HttpPost("version")]

        public async Task<IActionResult> AddVersion([FromBody] DitaTopicVersionViewModel ditaTopicVersion)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = new ReturnTypeOfDitaToics();

            // var ditaTopicViewModel = new DitaTopicVersionViewModel
            // {
            //     Id = ditaTopicVersion.DitaTopicId,
            //     VersionId = ditaTopicVersion.Id,
            //     DitaTopicId = ditaTopicVersion.DitaTopicId,
            //     ShortDescription = ditaTopicVersion.ShortDescription,
            //     VersionNumber = ditaTopicVersion.VersionNumber,
            //     Type = ditaTopicVersion.Type,
            //     Body = ditaTopicVersion.Body,
            //     Steps = ditaTopicVersion.Steps,
            //     Roles = ditaTopicVersion.Roles
            // };
            result = await AddUpdateDitaTopicAndVersions(ditaTopicVersion, AddDitaTopicVersion);

            return result.Code == 0 ? BadRequest(new { message = result.Message }) :
                Ok(new { message = result.Message, data = result.DitatopicVersion });
        }

        // PUT: api/DitaTopics/version/{id}
        [HttpPut("version/{id}")]
        public async Task<IActionResult> UpdateVersion([FromRoute] int id, [FromBody] DitaTopicVersionViewModel ditaTopicVersion)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // var ditaTopicViewModel = new DitaTopicModelView
            // {
            //     Id = ditaTopicVersion.DitaTopicId,
            //     VersionId = id,
            //     // DitaTopicId = ditaTopicVersion.DitaTopicId,
            //     ShortDescription = ditaTopicVersion.ShortDescription,
            //     VersionNumber = ditaTopicVersion.VersionNumber,
            //     Type = ditaTopicVersion.Type,
            //     Body = ditaTopicVersion.Body,
            //     Steps = ditaTopicVersion.Steps,
            //     Roles = ditaTopicVersion.Roles
            // }; 
            var result = await AddUpdateDitaTopicAndVersions(ditaTopicVersion, UpdateDitaTopicVersion, isUpdate: true);
            var dtVersion = await _context.DitatopicVersions.Include(x => x.DitaTopic).FirstOrDefaultAsync(x => x.Id == id);
            return result.Code == 0 ? BadRequest(new { message = result.Message }) :
                Ok(new
                {
                    message = result.Message,
                    data = new
                    {
                        dtVersion?.Id,
                        dtVersion?.ShortDescription,
                        dtVersion?.FileName,
                        dtVersion?.VersionNumber,
                        dtVersion?.CreatedAt,
                        dtVersion?.DitaTopicId,
                        Type = dtVersion is ConceptVersion ? 0 : dtVersion is TaskVersion ? 1 : -1,
                        body = dtVersion is ConceptVersion v1 ? v1.Body : null,
                        steps = dtVersion is TaskVersion v ? v.Steps.Select(s => new
                        {
                            s.Id,
                            s.Order,
                            s.Command,
                            s.TaskVersionId
                        }) : null,
                        Roles = dtVersion?.Roles.Select(x => x.RoleId)
                    }
                });
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

        //get all version of a DitaTopic
        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetDitaTopicVersions(int id)
        {
            var versions = await _context.DitatopicVersions
                        .Include(x => x.DitaTopic)
                        .ThenInclude(x => x.DocFamily)
                        .Select(dv => new
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
                        }).Where(x => x.DitaTopicId == id).ToListAsync();
            if (versions == null)
                return NotFound(new { message = $"DITA topic with ID {id} not found" });
            return Ok(versions);
        }
        private async Task<ReturnTypeOfDitaToics> AddUpdateDitaTopicAndVersions(DitaTopicVersionViewModel topic, GetDitaObject ditaObject, bool isUpdate = false)
        {
            var result = new ReturnTypeOfDitaToics();
            try
            {
                var family = await _context.DocFamilies.Include(x => x.DitaTopics).FirstOrDefaultAsync(x => x.Id == topic.DocFamilyId);
                using var transaction = await _context.Database.BeginTransactionAsync();

                result = await ditaObject(topic);
                if (result.DitatopicVersion != null)
                {
                    if (isUpdate) System.IO.File.Delete(result.DitatopicVersion.FilePath);
                    UpdateFileNameAndPathInDitatopicVersion(result.DitatopicVersion, result.Ditatopic!, (string)(family != null ? family?.FolderName! : result.Ditatopic!.DocFamily?.FolderName!));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                result.Code = 0;
                result.Message = ex.Message;
                return result;
            }
        }

        // private async Task<ReturnTypeOfDitaToics> AddConcept(DitaTopicModelView topic)
        // {
        //     var concept = new DitaTopic
        //     {
        //         Title = topic.Title,
        //         DocFamilyId = topic.DocFamilyId,
        //     };
        //     var version = new ConceptVersion
        //     {
        //         VersionNumber = topic.VersionNumber,
        //         ShortDescription = topic.ShortDescription,
        //         Body = topic.Body,
        //         Roles = topic.Roles.Select(x => new DitaTopicVersionsRoles { RoleId = x }).ToList()
        //     };
        //     concept.DitatopicVersions.Add(version);
        //     var result = await AddDitaTopic(concept);

        //     if (result.Code == 0)
        //         result.Message = "Failed to create Dita concept";
        //     else
        //     {
        //         result.Message = "Dita concept created successfully";
        //     }
        //     return result;
        // }

        // private async Task<ReturnTypeOfDitaToics> AddTask(DitaTopicModelView topic)
        // {
        //     var task = new DitaTopic
        //     {
        //         Title = topic.Title,
        //         DocFamilyId = topic.DocFamilyId
        //     };
        //     var version = new TaskVersion
        //     {
        //         ShortDescription = topic.ShortDescription,
        //         VersionNumber = topic.VersionNumber,
        //         Roles = topic.Roles.Select(x => new DitaTopicVersionsRoles { RoleId = x }).ToList()
        //     };
        //     foreach (var step in topic.Steps)
        //         version.Steps.Add(new Step { Order = step.Order, Command = step.Command });
        //     task.DitatopicVersions.Add(version);
        //     var result = await AddDitaTopic(task);
        //     if (result.Code == 0)
        //         result.Message = "Failed to create Dita task";
        //     else result.Message = "Dita task created successfully";

        //     return result;
        // }

        // private async Task<ReturnTypeOfDitaToics> UpdateDitaTopic(DitaTopicModelView topic)
        // {
        //     try
        //     {
        //         var result = new ReturnTypeOfDitaToics();
        //         var ditaTopic = await _context.DitaTopics.Include(x => x.DocFamily)

        //         .Include(x => x.DitatopicVersions).ThenInclude(r => r.Roles).FirstOrDefaultAsync(x => x.Id == topic.Id && x.DocFamilyId == topic.DocFamilyId);

        //         if (ditaTopic == null)
        //         {
        //             result.Code = 0;
        //             result.Message = "Dita topic not found";
        //             return result;
        //         }

        //         ditaTopic.Title = topic.Title;

        //         result.Code = await _context.SaveChangesAsync();

        //         foreach (var version in ditaTopic.DitatopicVersions)
        //         {
        //             var versiosnPath = $"{_hostingEnvironment.WebRootPath}\\{ditaTopic.Document?.FolderName}\\{_ditaFileCreationService.ReplaceInvalidChars(version.FileName)}.dita";

        //             System.IO.File.Delete(versiosnPath);
        //             if (version is TaskVersion)
        //             {
        //                 (version as TaskVersion).Steps = _context.Steps.Where(x => x.TaskVersionId == version.Id).ToList();
        //             }
        //             UpdateFileNameAndPathInDitatopicVersion(version, ditaTopic, ditaTopic.Document?.FolderName!);
        //         }
        //         result.Code = await _context.SaveChangesAsync();
        //         result.Ditatopic = ditaTopic;
        //         result.DitatopicVersion = null;
        //         if (result.Code == 0)
        //         {
        //             result.Message = "Failed to update Dita topic";
        //             return result;
        //         }
        //         result.Message = "Dita topic updated successfully";
        //         return result;
        //     }
        //     catch (Exception ex)
        //     {
        //         return new ReturnTypeOfDitaToics { Code = 0, Message = ex.Message };
        //     }
        // }
        private async Task<ReturnTypeOfDitaToics> AddDitaTopicVersion(DitaTopicVersionViewModel ditaTopicModelView)
        {
            var result = new ReturnTypeOfDitaToics();
            var ditaTopic = await _context.DitaTopics.Include(x => x.DocFamily).FirstOrDefaultAsync(x => x.Id == ditaTopicModelView.DitaTopicId);

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
                    DitaTopicId = ditaTopicModelView.DitaTopicId,
                    Body = ditaTopicModelView.Body,
                };
            }
            else
            {
                var taskVersion = new TaskVersion
                {
                    ShortDescription = ditaTopicModelView.ShortDescription,
                    VersionNumber = ditaTopicModelView.VersionNumber,
                    DitaTopicId = ditaTopicModelView.DitaTopicId,
                    Steps = ditaTopicModelView.Steps.Select(x => new Step
                    {
                        Order = x.Order,
                        Command = x.Command
                    }).ToList(),
                };

                version = taskVersion;
            }
            version.Roles = ditaTopicModelView.Roles.Select(x => new DitaTopicVersionsRoles { RoleId = x }).ToList();
            await _context.DitatopicVersions.AddAsync(version);

            result.Code = await _context.SaveChangesAsync();
            result.Message = "Dita topic version added successfully";
            result.DitatopicVersion = version;
            result.Ditatopic = ditaTopic;
            return result;
        }

        private async Task<ReturnTypeOfDitaToics> UpdateDitaTopicVersion(DitaTopicVersionViewModel ditaTopicModelView)
        {
            var result = new ReturnTypeOfDitaToics();
            var version = await _context.DitatopicVersions.
            Include(x => x.DitaTopic)
            .ThenInclude(x => x.DocFamily)
            .FirstOrDefaultAsync(x => x.Id == ditaTopicModelView.Id);
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
                var steps = _context.Steps.Where(x => x.TaskVersionId == version.Id).ToList();
                _context.Steps.RemoveRange(steps);
                await _context.SaveChangesAsync();
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
            var DocRoles = _context.DitaTopicVersionsRoles.Where(x => x.DitatopicVersionId == version.Id).ToList();

            _context.DitaTopicVersionsRoles.RemoveRange(DocRoles);
            result.Code = await _context.SaveChangesAsync();
            //update version roles
            var roles = ditaTopicModelView.Roles.Select(x => new DitaTopicVersionsRoles { RoleId = x }).ToList();
            version.Roles.Clear();
            version.Roles = roles;
            result.Code = await _context.SaveChangesAsync();
            result.Message = "Dita topic version updated successfully";
            result.DitatopicVersion = version;
            result.Ditatopic = version.DitaTopic;
            return result;
        }

        // private async Task<ReturnTypeOfDitaToics> AddDitaTopic(DitaTopic topic)
        // {
        //     try
        //     {
        //         await _context.DitaTopics.AddAsync(topic);
        //         await _context.SaveChangesAsync();

        //         return new ReturnTypeOfDitaToics { Code = 1, Ditatopic = topic, DitatopicVersion = topic.DitatopicVersions.FirstOrDefault() };
        //     }
        //     catch (Exception ex)
        //     {
        //         return new ReturnTypeOfDitaToics { Code = 0, Message = ex.Message };
        //     }
        // }


        private void UpdateFileNameAndPathInDitatopicVersion(DitatopicVersion version, dynamic ditaTopic, dynamic? docFolderName)
        {
            version.FileName = $"{ditaTopic.Title}_T_id_{ditaTopic.Id}_V_id_{version.Id}_v_no_{version.VersionNumber}";
            version.XmlContent = GenerateXml(version.FileName,
            ditaTopic.Title, version.ShortDescription,
             version is ConceptVersion ? ((ConceptVersion)version).Body : generateTaskStepsXml(((TaskVersion)version).Steps),
             version is ConceptVersion ? "concept" : "task");
            version.FilePath = _ditaFileCreationService.SaveDitaFile(version.XmlContent, docFolderName ?? "", version.FileName);
        }

        private string GenerateXml(string id, string title, string shortdesc, string body, string type = "concept")
        {
            var shorsDesc = string.IsNullOrEmpty(shortdesc) ? "" : $"<shortdesc>{shortdesc}</shortdesc>";
            id = id.Replace(" ", "_");
            return $@"<?xml version='1.0' encoding='UTF-8'?>
                <!DOCTYPE {type} PUBLIC '-//OASIS//DTD DITA {char.ToUpper(type[0]) + type[1..]}//EN' '../dtd/{type}.dtd'>
                <{type} id='{id}'>
                    <title>{title}</title>
                    {shorsDesc}
                    <{(type == "concept" ? "con" : "task")}body>
                        {body.Replace("<strong>", "<b>").Replace("</strong>", "</b>").Replace("<em>", "<i>").Replace("</em>", "</i>")}
                    </{(type == "concept" ? "con" : "task")}body>
                </{type}>
            ";
        }

        private string generateTaskStepsXml(ICollection<Step> steps)
        {
            StringBuilder stepsXml = new StringBuilder("<steps>");
            foreach (var step in steps.OrderBy(x => x.Order))
            {
                stepsXml.Append(@"<step>
                    <cmd>").Append(step.Command).Append(@"</cmd>
                </step>");
            }
            stepsXml.Append("</steps>");
            return stepsXml.ToString();
        }
    }
}