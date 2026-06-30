using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GasMapQuebec.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_corrections",
                schema: "pricing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SubmittedPriceCents = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    PreviousPriceCents = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    PercentChange = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SubmitterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_corrections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_price_corrections_StationId_FuelType_Status",
                schema: "pricing",
                table: "price_corrections",
                columns: new[] { "StationId", "FuelType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_price_corrections_Status",
                schema: "pricing",
                table: "price_corrections",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_price_corrections_SubmitterId_SubmittedAtUtc",
                schema: "pricing",
                table: "price_corrections",
                columns: new[] { "SubmitterId", "SubmittedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_corrections",
                schema: "pricing");
        }
    }
}
