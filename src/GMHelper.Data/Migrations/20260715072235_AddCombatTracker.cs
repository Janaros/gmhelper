using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCombatTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CombatEncounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CampaignId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentRound = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentTurnParticipantId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatEncounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombatEncounters_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CombatParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CombatEncounterId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: true),
                    MonsterId = table.Column<int>(type: "INTEGER", nullable: true),
                    Initiative = table.Column<int>(type: "INTEGER", nullable: true),
                    CurrentTrackedValue = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxTrackedValue = table.Column<int>(type: "INTEGER", nullable: true),
                    ConditionsText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombatParticipants_CombatEncounters_CombatEncounterId",
                        column: x => x.CombatEncounterId,
                        principalTable: "CombatEncounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CombatEncounters_CampaignId_IsActive",
                table: "CombatEncounters",
                columns: new[] { "CampaignId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CombatParticipants_CombatEncounterId",
                table: "CombatParticipants",
                column: "CombatEncounterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CombatParticipants");

            migrationBuilder.DropTable(
                name: "CombatEncounters");
        }
    }
}
