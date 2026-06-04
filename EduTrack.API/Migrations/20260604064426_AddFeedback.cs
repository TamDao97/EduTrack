using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTutor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifyUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifyUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModify = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");
        }
    }
}
