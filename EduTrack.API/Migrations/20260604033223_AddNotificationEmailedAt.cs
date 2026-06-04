using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationEmailedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailedAt",
                table: "Notifications");
        }
    }
}
