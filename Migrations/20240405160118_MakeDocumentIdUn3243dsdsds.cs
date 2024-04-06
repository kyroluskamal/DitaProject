using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class MakeDocumentIdUn3243dsdsds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_DitaTopics_TaskId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "DitaTopics");

            migrationBuilder.DropColumn(
                name: "Reference_Body",
                table: "DitaTopics");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Steps",
                newName: "TaskVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_Steps_TaskId",
                table: "Steps",
                newName: "IX_Steps_TaskVersionId");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "DitatopicVersions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "DitatopicVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocVersionId",
                table: "DitaTopics",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DitaTopics_DocVersionId",
                table: "DitaTopics",
                column: "DocVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DitaTopics_DocVersions_DocVersionId",
                table: "DitaTopics",
                column: "DocVersionId",
                principalTable: "DocVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_DitatopicVersions_TaskVersionId",
                table: "Steps",
                column: "TaskVersionId",
                principalTable: "DitatopicVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DitaTopics_DocVersions_DocVersionId",
                table: "DitaTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Steps_DitatopicVersions_TaskVersionId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_DitaTopics_DocVersionId",
                table: "DitaTopics");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "DitatopicVersions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DitatopicVersions");

            migrationBuilder.DropColumn(
                name: "DocVersionId",
                table: "DitaTopics");

            migrationBuilder.RenameColumn(
                name: "TaskVersionId",
                table: "Steps",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Steps_TaskVersionId",
                table: "Steps",
                newName: "IX_Steps_TaskId");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "DitaTopics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference_Body",
                table: "DitaTopics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_DitaTopics_TaskId",
                table: "Steps",
                column: "TaskId",
                principalTable: "DitaTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
