using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class MakeDocumentIdUn3243 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions");

            migrationBuilder.AddForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions",
                column: "DitaTopicId",
                principalTable: "DitaTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions");

            migrationBuilder.AddForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions",
                column: "DitaTopicId",
                principalTable: "DitaTopics",
                principalColumn: "Id");
        }
    }
}
