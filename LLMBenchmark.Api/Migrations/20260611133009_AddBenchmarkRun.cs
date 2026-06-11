using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBenchmarkRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatencyMs",
                table: "BenchmarkResults",
                newName: "EndToEndLatencyMs");

            migrationBuilder.AddColumn<Guid>(
                name: "BenchmarkRunId",
                table: "BenchmarkResults",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "ProviderLatencyMs",
                table: "BenchmarkResults",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawResponse",
                table: "BenchmarkResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "BenchmarkResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "BenchmarkRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalScenarios = table.Column<int>(type: "integer", nullable: false),
                    TotalExecutions = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenchmarkRuns", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenchmarkRuns");

            migrationBuilder.DropColumn(
                name: "BenchmarkRunId",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "ProviderLatencyMs",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "RawResponse",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "BenchmarkResults");

            migrationBuilder.RenameColumn(
                name: "EndToEndLatencyMs",
                table: "BenchmarkResults",
                newName: "LatencyMs");
        }
    }
}
