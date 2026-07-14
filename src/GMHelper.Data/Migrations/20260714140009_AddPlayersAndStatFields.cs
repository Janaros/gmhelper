using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayersAndStatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CampaignId = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Initiative = table.Column<int>(type: "INTEGER", nullable: true),
                    PortraitImageAssetId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatFields", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_CampaignId",
                table: "Players",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_StatFields_OwnerType_OwnerId_SortOrder",
                table: "StatFields",
                columns: new[] { "OwnerType", "OwnerId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "StatFields");
        }
    }
}
