using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "TaskComments");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "TaskComments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TaskComments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TaskComments");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "TaskComments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
