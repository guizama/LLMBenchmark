using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBenchmarkValidationResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "InputTokenErrorPercent",
                table: "BenchmarkValidationResults",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemPrompt",
                table: "BenchmarkResults",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputTokenErrorPercent",
                table: "BenchmarkValidationResults");

            migrationBuilder.DropColumn(
                name: "SystemPrompt",
                table: "BenchmarkResults");
        }
    }
}
