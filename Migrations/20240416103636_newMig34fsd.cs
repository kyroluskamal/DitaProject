using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class newMig34fsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopicVersionsRoles_DitatopicVersions_DitatopicVersionId",
                table: "DitaTopicVersionsRoles");

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopicVersionsRoles_DitatopicVersions_DitatopicVersionId",
                table: "DitaTopicVersionsRoles",
                column: "DitatopicVersionId",
                principalTable: "DitatopicVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopicVersionsRoles_DitatopicVersions_DitatopicVersionId",
                table: "DitaTopicVersionsRoles");

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopicVersionsRoles_DitatopicVersions_DitatopicVersionId",
                table: "DitaTopicVersionsRoles",
                column: "DitatopicVersionId",
                principalTable: "DitatopicVersions",
                principalColumn: "Id");
        }
    }
}
