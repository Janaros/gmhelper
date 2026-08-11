using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHerbalism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HerbalismRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Terrain = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    DifficultyClass = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HerbalismRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HerbalismIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HerbalismRegionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    Effect = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ValueInGoldPieces = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HerbalismIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HerbalismIngredients_HerbalismRegions_HerbalismRegionId",
                        column: x => x.HerbalismRegionId,
                        principalTable: "HerbalismRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HerbalismIngredients_HerbalismRegionId_Name",
                table: "HerbalismIngredients",
                columns: new[] { "HerbalismRegionId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_HerbalismRegions_Name",
                table: "HerbalismRegions",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HerbalismIngredients");

            migrationBuilder.DropTable(
                name: "HerbalismRegions");
        }
    }
}
