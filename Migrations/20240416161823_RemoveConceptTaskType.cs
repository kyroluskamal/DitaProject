﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppConfgDocumentation.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConceptTaskType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "DitaTopics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "DitaTopics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
