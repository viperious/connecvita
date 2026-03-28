using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Connecvita.Infrastructure.DataMigrations
{
    /// <inheritdoc />
    public partial class AddExtendedWearableMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecoveryHigh",
                table: "WearableMetrics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SleepEfficiency",
                table: "WearableMetrics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SpO2",
                table: "WearableMetrics",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StressHigh",
                table: "WearableMetrics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StressSummary",
                table: "WearableMetrics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSleepMinutes",
                table: "WearableMetrics",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecoveryHigh",
                table: "WearableMetrics");

            migrationBuilder.DropColumn(
                name: "SleepEfficiency",
                table: "WearableMetrics");

            migrationBuilder.DropColumn(
                name: "SpO2",
                table: "WearableMetrics");

            migrationBuilder.DropColumn(
                name: "StressHigh",
                table: "WearableMetrics");

            migrationBuilder.DropColumn(
                name: "StressSummary",
                table: "WearableMetrics");

            migrationBuilder.DropColumn(
                name: "TotalSleepMinutes",
                table: "WearableMetrics");
        }
    }
}
