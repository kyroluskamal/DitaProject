using AppConfgDocumentation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppConfgDocumentation.Data
{
    using Microsoft.AspNetCore.Identity.Data;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,
        IdentityRole<int>, int, IdentityUserClaim<int>, IdentityUserRole<int>,
        IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DitaTopic> DitaTopics { get; set; }
        public DbSet<Concept> Concepts { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Reference> References { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<DocVersion> DocVersions { get; set; }
        public DbSet<DitatopicVersion> DitatopicVersions { get; set; }
        public DbSet<DocVersionDitatopicVersion> DocVersionDitatopicVersions { get; set; }
        public DbSet<ConceptVersion> ConceptVersions { get; set; }
        public DbSet<TaskVersion> TaskVersions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DitaTopic>()
            .HasDiscriminator<int>("Type")
            .HasValue<Concept>(0)
            .HasValue<Tasks>(1)
            .HasValue<Reference>(2);



            // Configure the primary key for DocVersionDitatopicVersion
            modelBuilder.Entity<DocVersionDitatopicVersion>()
                .HasKey(dvdt => new { dvdt.DocVersionId, dvdt.DitatopicVersionId });

            // Configure the many-to-many relationship between DocVersion and DitatopicVersion
            modelBuilder.Entity<DocVersionDitatopicVersion>()
                .HasOne(dvdt => dvdt.DocVersion)
                .WithMany(dv => dv.DitatopicVersions)
                .HasForeignKey(dvdt => dvdt.DocVersionId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<DitatopicVersion>()
                .HasDiscriminator<int>("Type")
                .HasValue<ConceptVersion>(0)
                .HasValue<TaskVersion>(1);
            modelBuilder.Entity<DocVersionDitatopicVersion>()
                .HasOne(dvdt => dvdt.DitatopicVersion)
                .WithMany(dt => dt.DocVersions)
                .HasForeignKey(dvdt => dvdt.DitatopicVersionId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Documento>()
                .HasMany(d => d.DocVersions)
                .WithOne(v => v.Document)
                .HasForeignKey(v => v.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DitatopicVersion>()
                .HasMany(dtv => dtv.DocVersions)
                .WithOne(dv => dv.DitatopicVersion)
                .HasForeignKey(dv => dv.DitatopicVersionId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<DitaTopic>()
                .HasMany(dt => dt.DitatopicVersions)
                .WithOne(dv => dv.DitaTopic)
                .HasForeignKey(dv => dv.DitaTopicId)
                .OnDelete(DeleteBehavior.Cascade);

            // modelBuilder.Entity<ConceptVersion>()
            // .HasOne(cv => cv.DitaTopic)
            // .WithMany(dt => dt.DitatopicVersions)
            // .HasForeignKey(cv => cv.DitaTopicId);

            // modelBuilder.Entity<TaskVersion>()
            //     .HasOne(tv => tv.DitaTopic)
            //     .WithMany(dt => dt.DitatopicVersions)
            //     .HasForeignKey(tv => tv.DitaTopicId);

            modelBuilder.Entity<Step>()
                .HasOne(s => s.TaskVersion)
                .WithMany(tv => tv.Steps)
                .HasForeignKey(s => s.TaskVersionId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
