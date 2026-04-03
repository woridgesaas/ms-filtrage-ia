using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ms_filtering_ia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriteriaTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriteriaName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilteringProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilteringProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilteringProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilteringProfile_CriteriaType",
                columns: table => new
                {
                    CriteriaTypesId = table.Column<int>(type: "int", nullable: false),
                    FilteringProfilesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilteringProfile_CriteriaType", x => new { x.CriteriaTypesId, x.FilteringProfilesId });
                    table.ForeignKey(
                        name: "FK_FilteringProfile_CriteriaType_CriteriaTypes_CriteriaTypesId",
                        column: x => x.CriteriaTypesId,
                        principalTable: "CriteriaTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilteringProfile_CriteriaType_FilteringProfiles_FilteringProfilesId",
                        column: x => x.FilteringProfilesId,
                        principalTable: "FilteringProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilteringResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CandidateEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CandidatePhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GlobalScore = table.Column<double>(type: "float", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilteringProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilteringResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilteringResults_FilteringProfiles_FilteringProfileId",
                        column: x => x.FilteringProfileId,
                        principalTable: "FilteringProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilteringProfile_CriteriaType_FilteringProfilesId",
                table: "FilteringProfile_CriteriaType",
                column: "FilteringProfilesId");

            migrationBuilder.CreateIndex(
                name: "IX_FilteringProfiles_UserId",
                table: "FilteringProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FilteringResults_FilteringProfileId",
                table: "FilteringResults",
                column: "FilteringProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilteringProfile_CriteriaType");

            migrationBuilder.DropTable(
                name: "FilteringResults");

            migrationBuilder.DropTable(
                name: "CriteriaTypes");

            migrationBuilder.DropTable(
                name: "FilteringProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
