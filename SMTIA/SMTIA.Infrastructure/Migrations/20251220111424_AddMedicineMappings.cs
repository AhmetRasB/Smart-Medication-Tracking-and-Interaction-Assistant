using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMTIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicineMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicineMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QueryTerm = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    BrandNameTr = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ActiveIngredientTr = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ActiveIngredientEn = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineMappings_Users_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineMappings_ConfirmedByUserId",
                table: "MedicineMappings",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineMappings_QueryTerm",
                table: "MedicineMappings",
                column: "QueryTerm");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineMappings_Status_Source",
                table: "MedicineMappings",
                columns: new[] { "Status", "Source" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicineMappings");
        }
    }
}
