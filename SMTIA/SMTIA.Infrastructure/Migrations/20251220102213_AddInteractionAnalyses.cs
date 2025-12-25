using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMTIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInteractionAnalyses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractionAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewMedicineId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewMedicineName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ExistingMedicinesJson = table.Column<string>(type: "text", nullable: false),
                    AllergiesJson = table.Column<string>(type: "text", nullable: true),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    DetailedAnalysis = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    RawAiResponse = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionAnalyses_Medicines_NewMedicineId",
                        column: x => x.NewMedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InteractionAnalyses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InteractionAnalyses_NewMedicineId",
                table: "InteractionAnalyses",
                column: "NewMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionAnalyses_RiskLevel",
                table: "InteractionAnalyses",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionAnalyses_UserId_CreatedAt",
                table: "InteractionAnalyses",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractionAnalyses");
        }
    }
}
