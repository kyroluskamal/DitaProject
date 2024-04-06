using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class MakeDocumentIdUn32 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "DitaTopics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics",
                column: "DocumentId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "DitaTopics",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopics_Documentos_DocumentId",
                table: "DitaTopics",
                column: "DocumentId",
                principalTable: "Documentos",
                principalColumn: "Id");
        }
    }
}
