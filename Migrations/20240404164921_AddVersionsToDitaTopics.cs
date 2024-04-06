using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionsToDitaTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VersionSections");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Documentos");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Documentos",
                newName: "FolderName");

            migrationBuilder.CreateTable(
                name: "DitaTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reference_Body = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DitaTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DitaTopics_Documentos_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VersionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DitaMapXml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DitaMapFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocVersions_Documentos_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DitatopicVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    XmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VersionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DitaTopicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DitatopicVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                        column: x => x.DitaTopicId,
                        principalTable: "DitaTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Command = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_DitaTopics_TaskId",
                        column: x => x.TaskId,
                        principalTable: "DitaTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocVersionDitatopicVersions",
                columns: table => new
                {
                    DocVersionId = table.Column<int>(type: "int", nullable: false),
                    DitatopicVersionId = table.Column<int>(type: "int", nullable: false),
                    DitaTopicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocVersionDitatopicVersions", x => new { x.DocVersionId, x.DitatopicVersionId });
                    table.ForeignKey(
                        name: "FK_DocVersionDitatopicVersions_DitaTopics_DitaTopicId",
                        column: x => x.DitaTopicId,
                        principalTable: "DitaTopics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocVersionDitatopicVersions_DitatopicVersions_DitatopicVersionId",
                        column: x => x.DitatopicVersionId,
                        principalTable: "DitatopicVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocVersionDitatopicVersions_DocVersions_DocVersionId",
                        column: x => x.DocVersionId,
                        principalTable: "DocVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DitaTopics_DocumentId",
                table: "DitaTopics",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DitatopicVersions_DitaTopicId",
                table: "DitatopicVersions",
                column: "DitaTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_DocVersionDitatopicVersions_DitaTopicId",
                table: "DocVersionDitatopicVersions",
                column: "DitaTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_DocVersionDitatopicVersions_DitatopicVersionId",
                table: "DocVersionDitatopicVersions",
                column: "DitatopicVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocVersions_DocumentId",
                table: "DocVersions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_TaskId",
                table: "Steps",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocVersionDitatopicVersions");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "DitatopicVersions");

            migrationBuilder.DropTable(
                name: "DocVersions");

            migrationBuilder.DropTable(
                name: "DitaTopics");

            migrationBuilder.RenameColumn(
                name: "FolderName",
                table: "Documentos",
                newName: "Description");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Documentos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ditafileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VersionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Versions_Documentos_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VersionSections",
                columns: table => new
                {
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionSections", x => new { x.VersionId, x.SectionId });
                    table.ForeignKey(
                        name: "FK_VersionSections_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VersionSections_Versions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "Versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Versions_DocumentId",
                table: "Versions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_VersionSections_SectionId",
                table: "VersionSections",
                column: "SectionId");
        }
    }
}
