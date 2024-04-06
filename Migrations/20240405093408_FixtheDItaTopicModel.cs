using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class FixtheDItaTopicModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_DocVersionDitatopicVersions_DitaTopics_DitaTopicId",
                table: "DocVersionDitatopicVersions");

            migrationBuilder.DropIndex(
                name: "IX_DocVersionDitatopicVersions_DitaTopicId",
                table: "DocVersionDitatopicVersions");

            migrationBuilder.DropColumn(
                name: "DitaTopicId",
                table: "DocVersionDitatopicVersions");

            migrationBuilder.AddForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions",
                column: "DitaTopicId",
                principalTable: "DitaTopics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions");

            migrationBuilder.AddColumn<int>(
                name: "DitaTopicId",
                table: "DocVersionDitatopicVersions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocVersionDitatopicVersions_DitaTopicId",
                table: "DocVersionDitatopicVersions",
                column: "DitaTopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_DitatopicVersions_DitaTopics_DitaTopicId",
                table: "DitatopicVersions",
                column: "DitaTopicId",
                principalTable: "DitaTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocVersionDitatopicVersions_DitaTopics_DitaTopicId",
                table: "DocVersionDitatopicVersions",
                column: "DitaTopicId",
                principalTable: "DitaTopics",
                principalColumn: "Id");
        }
    }
}
