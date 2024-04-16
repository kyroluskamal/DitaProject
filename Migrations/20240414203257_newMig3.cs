using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class newMig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocVersionsRoles_DocVersions_DocVersionId",
                table: "DocVersionsRoles");

            migrationBuilder.AddForeignKey(
                name: "FK_DocVersionsRoles_DocVersions_DocVersionId",
                table: "DocVersionsRoles",
                column: "DocVersionId",
                principalTable: "DocVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocVersionsRoles_DocVersions_DocVersionId",
                table: "DocVersionsRoles");

            migrationBuilder.AddForeignKey(
                name: "FK_DocVersionsRoles_DocVersions_DocVersionId",
                table: "DocVersionsRoles",
                column: "DocVersionId",
                principalTable: "DocVersions",
                principalColumn: "Id");
        }
    }
}
