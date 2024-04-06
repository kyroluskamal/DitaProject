using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class MakeDocumentIdUnrequiredInDB2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics");

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics",
                column: "DocumentId",
                principalTable: "Documentos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics");

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
