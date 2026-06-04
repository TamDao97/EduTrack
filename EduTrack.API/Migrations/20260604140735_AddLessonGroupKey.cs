using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonGroupKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupKey",
                table: "Lessons",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupKey",
                table: "Lessons");
        }
    }
}
