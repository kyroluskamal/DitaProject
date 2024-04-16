using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class newMig34 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DitaMapFilePath",
                table: "DocVersions");

            migrationBuilder.DropColumn(
                name: "DitaMapXml",
                table: "DocVersions");

            migrationBuilder.DropColumn(
                name: "PDFfilePath",
                table: "DocVersions");

            migrationBuilder.AddColumn<string>(
                name: "DitaMapXml",
                table: "DocVersionsRoles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DitaMapXml",
                table: "DocVersionsRoles");

            migrationBuilder.AddColumn<string>(
                name: "DitaMapFilePath",
                table: "DocVersions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DitaMapXml",
                table: "DocVersions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PDFfilePath",
                table: "DocVersions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
