using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class FixTuitionPeriodUniqueIndexSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TuitionPeriods_IdStudent_PeriodYear_PeriodMonth",
                table: "TuitionPeriods");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPeriods_IdStudent_PeriodYear_PeriodMonth",
                table: "TuitionPeriods",
                columns: new[] { "IdStudent", "PeriodYear", "PeriodMonth" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TuitionPeriods_IdStudent_PeriodYear_PeriodMonth",
                table: "TuitionPeriods");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPeriods_IdStudent_PeriodYear_PeriodMonth",
                table: "TuitionPeriods",
                columns: new[] { "IdStudent", "PeriodYear", "PeriodMonth" },
                unique: true);
        }
    }
}
