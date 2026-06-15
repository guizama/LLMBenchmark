using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOutputEstimatedSmsSegmentsQtd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OutputEstimatedSmsSegmentsQtd",
                table: "BenchmarkResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputEstimatedSmsSegmentsQtd",
                table: "BenchmarkResults");
        }
    }
}
