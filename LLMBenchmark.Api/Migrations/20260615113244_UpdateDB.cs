using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capability",
                table: "BenchmarkResults");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "BenchmarkResults",
                newName: "Action");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Action",
                table: "BenchmarkResults",
                newName: "Category");

            migrationBuilder.AddColumn<string>(
                name: "Capability",
                table: "BenchmarkResults",
                type: "text",
                nullable: true);
        }
    }
}
