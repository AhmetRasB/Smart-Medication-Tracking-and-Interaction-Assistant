using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMTIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleTimingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduleTimings_MedicationScheduleId",
                table: "ScheduleTimings");

            migrationBuilder.AlterColumn<int>(
                name: "DayOfWeek",
                table: "ScheduleTimings",
                type: "integer",
                nullable: true,
                comment: "0=Pazar, 1=Pazartesi, ..., 6=Cumartesi (null = her gün)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimings_MedicationScheduleId_Time",
                table: "ScheduleTimings",
                columns: new[] { "MedicationScheduleId", "Time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduleTimings_MedicationScheduleId_Time",
                table: "ScheduleTimings");

            migrationBuilder.AlterColumn<int>(
                name: "DayOfWeek",
                table: "ScheduleTimings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "0=Pazar, 1=Pazartesi, ..., 6=Cumartesi (null = her gün)");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimings_MedicationScheduleId",
                table: "ScheduleTimings",
                column: "MedicationScheduleId");
        }
    }
}
