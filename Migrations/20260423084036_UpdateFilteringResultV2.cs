using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ms_filtering_ia.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFilteringResultV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassificationConfidence",
                table: "FilteringResults");

            migrationBuilder.RenameColumn(
                name: "DetectedCategory",
                table: "FilteringResults",
                newName: "SeniorityLevel");

            migrationBuilder.AddColumn<string>(
                name: "CvId",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CvStatus",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MainDomain",
                table: "FilteringResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvId",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "CvStatus",
                table: "FilteringResults");

            migrationBuilder.DropColumn(
                name: "MainDomain",
                table: "FilteringResults");

            migrationBuilder.RenameColumn(
                name: "SeniorityLevel",
                table: "FilteringResults",
                newName: "DetectedCategory");

            migrationBuilder.AddColumn<double>(
                name: "ClassificationConfidence",
                table: "FilteringResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
