using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class FamilyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "DitaTopics",
                newName: "DocFamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_DitaTopics_DocumentId",
                table: "DitaTopics",
                newName: "IX_DitaTopics_DocFamilyId");

            migrationBuilder.AddColumn<int>(
                name: "DocFamilyId",
                table: "Documentos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "DitaTopics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DocFamilies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocFamilies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_DocFamilyId",
                table: "Documentos",
                column: "DocFamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopics_DocFamilies_DocFamilyId",
                table: "DitaTopics",
                column: "DocFamilyId",
                principalTable: "DocFamilies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_DocFamilies_DocFamilyId",
                table: "Documentos",
                column: "DocFamilyId",
                principalTable: "DocFamilies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopics_DocFamilies_DocFamilyId",
                table: "DitaTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_DocFamilies_DocFamilyId",
                table: "Documentos");

            migrationBuilder.DropTable(
                name: "DocFamilies");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_DocFamilyId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "DocFamilyId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "DitaTopics");

            migrationBuilder.RenameColumn(
                name: "DocFamilyId",
                table: "DitaTopics",
                newName: "DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_DitaTopics_DocFamilyId",
                table: "DitaTopics",
                newName: "IX_DitaTopics_DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics",
                column: "DocumentId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
