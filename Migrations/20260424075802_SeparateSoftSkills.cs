using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ms_filtering_ia.Migrations
{
    /// <inheritdoc />
    public partial class SeparateSoftSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtractedSoftSkills",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedSoftSkills",
                table: "FilteringResults");
        }
    }
}
