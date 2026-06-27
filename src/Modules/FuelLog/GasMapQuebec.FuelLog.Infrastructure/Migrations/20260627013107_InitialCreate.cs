using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GasMapQuebec.FuelLog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fuellog");

            migrationBuilder.CreateTable(
                name: "entries",
                schema: "fuellog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FuelGrade = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Litres = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    OdometerKm = table.Column<int>(type: "integer", nullable: true),
                    StationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entries_UserId_FilledAtUtc",
                schema: "fuellog",
                table: "entries",
                columns: new[] { "UserId", "FilledAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entries",
                schema: "fuellog");
        }
    }
}
