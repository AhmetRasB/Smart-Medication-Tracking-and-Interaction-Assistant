using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMTIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserPrescriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserPrescriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserDiseases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserDiseases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserAllergies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserAllergies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SideEffects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SideEffects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ScheduleTimings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ScheduleTimings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PrescriptionMedicines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PrescriptionMedicines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MedicineSideEffects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicineSideEffects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Medicines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Medicines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MedicationSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicationSchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "IntakeLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "IntakeLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserPrescriptions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserPrescriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserDiseases");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserDiseases");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserAllergies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserAllergies");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SideEffects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SideEffects");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ScheduleTimings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ScheduleTimings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PrescriptionMedicines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PrescriptionMedicines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MedicineSideEffects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MedicineSideEffects");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MedicationSchedules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MedicationSchedules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "IntakeLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "IntakeLogs");
        }
    }
}
