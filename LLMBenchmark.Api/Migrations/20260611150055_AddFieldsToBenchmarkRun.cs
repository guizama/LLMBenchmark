using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToBenchmarkRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "BenchmarkRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BenchmarkRuns",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "BenchmarkRuns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BenchmarkRuns");
        }
    }
}
