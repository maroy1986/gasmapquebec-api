using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GasMapQuebec.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pricing");

            migrationBuilder.CreateTable(
                name: "stations",
                schema: "pricing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoordinateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Brand = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fuel_prices",
                schema: "pricing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PriceCents = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    ObservedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fuel_prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fuel_prices_stations_StationId",
                        column: x => x.StationId,
                        principalSchema: "pricing",
                        principalTable: "stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fuel_prices_StationId_FuelType",
                schema: "pricing",
                table: "fuel_prices",
                columns: new[] { "StationId", "FuelType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stations_CoordinateKey",
                schema: "pricing",
                table: "stations",
                column: "CoordinateKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fuel_prices",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "stations",
                schema: "pricing");
        }
    }
}
