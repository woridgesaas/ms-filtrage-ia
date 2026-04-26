using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ms_filtering_ia.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingSkillsAndRecommendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FilteringProfileId",
                table: "FilteringResults",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "MissingSkills",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Recommendation",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MissingSkills",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "FilteringResults");

            migrationBuilder.AlterColumn<int>(
                name: "FilteringProfileId",
                table: "FilteringResults",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
