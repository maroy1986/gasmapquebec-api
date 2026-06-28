using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GasMapQuebec.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_history",
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
                    table.PrimaryKey("PK_price_history", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_price_history_StationId_FuelType_ObservedAtUtc",
                schema: "pricing",
                table: "price_history",
                columns: new[] { "StationId", "FuelType", "ObservedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_history",
                schema: "pricing");
        }
    }
}
