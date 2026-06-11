using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBenchmark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Capability",
                table: "BenchmarkResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BenchmarkResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "BenchmarkResults",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "BenchmarkResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OutputCharacters",
                table: "BenchmarkResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PredictedInputTokens",
                table: "BenchmarkResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SmsSegments",
                table: "BenchmarkResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "BenchmarkResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capability",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "OutputCharacters",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "PredictedInputTokens",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "SmsSegments",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "BenchmarkResults");
        }
    }
}
