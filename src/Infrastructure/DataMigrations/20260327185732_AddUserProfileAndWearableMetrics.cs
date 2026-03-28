using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Connecvita.Infrastructure.DataMigrations
{
    /// <inheritdoc />
    public partial class AddUserProfileAndWearableMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PhysicalAttributes_ChestCm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicalAttributes_WaistCm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicalAttributes_HipsCm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneticData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BloodType = table.Column<int>(type: "int", nullable: true),
                    SleepScore = table.Column<int>(type: "int", nullable: true),
                    ReadinessScore = table.Column<int>(type: "int", nullable: true),
                    ActivityScore = table.Column<int>(type: "int", nullable: true),
                    RestingHeartRate = table.Column<double>(type: "float", nullable: true),
                    HRV = table.Column<double>(type: "float", nullable: true),
                    BodyTemperature = table.Column<double>(type: "float", nullable: true),
                    HealthScoresLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MatchingContext = table.Column<int>(type: "int", nullable: false),
                    MatchingPreferences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrls = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoiceProfileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectedPlatforms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WearableMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    SleepScore = table.Column<int>(type: "int", nullable: true),
                    ReadinessScore = table.Column<int>(type: "int", nullable: true),
                    ActivityScore = table.Column<int>(type: "int", nullable: true),
                    HeartRate = table.Column<double>(type: "float", nullable: true),
                    HRV = table.Column<double>(type: "float", nullable: true),
                    Temperature = table.Column<double>(type: "float", nullable: true),
                    WorkoutSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WearableMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WearableMetrics_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WearableMetrics_UserProfileId_RecordedAt",
                table: "WearableMetrics",
                columns: new[] { "UserProfileId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WearableMetrics");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
