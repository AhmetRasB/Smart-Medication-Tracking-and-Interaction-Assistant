using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMTIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mg10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntervalHours",
                table: "ScheduleTimings",
                type: "integer",
                nullable: true,
                comment: "Interval tipi için saat aralığı (örn: 12 = her 12 saatte bir)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalHours",
                table: "ScheduleTimings");
        }
    }
}
