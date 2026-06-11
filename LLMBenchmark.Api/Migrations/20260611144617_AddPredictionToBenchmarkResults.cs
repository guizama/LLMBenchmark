using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionToBenchmarkResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InputTokenDelta",
                table: "BenchmarkResults",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "InputTokenErrorPercent",
                table: "BenchmarkResults",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenEstimator",
                table: "BenchmarkResults",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputTokenDelta",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "InputTokenErrorPercent",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "TokenEstimator",
                table: "BenchmarkResults");
        }
    }
}
