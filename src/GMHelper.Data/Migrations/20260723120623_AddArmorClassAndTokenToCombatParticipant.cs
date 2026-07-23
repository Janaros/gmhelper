using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArmorClassAndTokenToCombatParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArmorClass",
                table: "CombatParticipants",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenNumber",
                table: "CombatParticipants",
                type: "TEXT",
                maxLength: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArmorClass",
                table: "CombatParticipants");

            migrationBuilder.DropColumn(
                name: "TokenNumber",
                table: "CombatParticipants");
        }
    }
}
