﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AppConfgDocumentation.Models
{
    public class DitaTopic : CommonModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(DocFamilyId))]
        public int DocFamilyId { get; set; }
        public virtual DocFamily? DocFamily { get; set; }
        public virtual ICollection<DitatopicVersion> DitatopicVersions { get; set; } = [];

    }
    public abstract class DitatopicVersion : CommonModel
    {
        public string? ShortDescription { get; set; } = string.Empty;
        public string XmlContent { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string VersionNumber { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        [Required]
        [ForeignKey(nameof(DitaTopicId))]
        public int DitaTopicId { get; set; }
        public virtual DitaTopic? DitaTopic { get; set; }
        public virtual ICollection<DocVersionDitatopicVersion> DocVersions { get; set; } = [];
        public virtual ICollection<DitaTopicVersionsRoles> Roles { get; set; } = [];

    }
    // public class Concept : DitaTopic
    // {
    // }

    // public class Tasks : DitaTopic
    // {
    // }

    // public class Reference : DitaTopic
    // {
    // }
    public class ConceptVersion : DitatopicVersion
    {
        public string Body { get; set; } = string.Empty;
    }

    public class TaskVersion : DitatopicVersion
    {
        public virtual ICollection<Step> Steps { get; set; } = new List<Step>();
    }
    public class ReferenceVersion : DitatopicVersion
    {
        public string Body { get; set; } = string.Empty;
    }
    public class Step : CommonModel
    {
        public int Order { get; set; }
        public string Command { get; set; } = string.Empty;
        [ForeignKey(nameof(TaskVersion))]
        public int TaskVersionId { get; set; }
        public virtual TaskVersion? TaskVersion { get; set; }
    }

    public class DitaTopicVersionsRoles
    {
        public int RoleId { get; set; }
        public virtual IdentityRole<int> Role { get; set; }
        public int DitatopicVersionId { get; set; }
        public virtual DitatopicVersion DitatopicVersion { get; set; }
    }
}

