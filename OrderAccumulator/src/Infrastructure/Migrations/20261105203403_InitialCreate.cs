using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetExposures",
                columns: table => new
                {
                    Asset = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ExposureCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetExposures", x => x.Asset);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Asset = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Side = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AssetExposures_Asset",
                        column: x => x.Asset,
                        principalTable: "AssetExposures",
                        principalColumn: "Asset",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AssetExposures",
                columns: new[] { "Asset", "ExposureCents", "Version" },
                values: new object[,]
                {
                    { "PETR4", 0L, 0 },
                    { "VALE3", 0L, 0 },
                    { "VIIA4", 0L, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Asset_CreatedAt",
                table: "Orders",
                columns: new[] { "Asset", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "AssetExposures");
        }
    }
}
