using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ms_filtering_ia.Migrations
{
    /// <inheritdoc />
    public partial class AddAiFieldsToFilteringResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CandidateLocation",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ClassificationConfidence",
                table: "FilteringResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CvMinioPath",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DetectedCategory",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "FilteringResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedSkills",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateLocation",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "ClassificationConfidence",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "CvMinioPath",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "DetectedCategory",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "ExtractedSkills",
                table: "FilteringResults");
        }
    }
}
