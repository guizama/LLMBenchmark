using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMBenchmark.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinksOk",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "PlaceholdersOk",
                table: "BenchmarkResults");

            migrationBuilder.DropColumn(
                name: "SmsSegments",
                table: "BenchmarkResults");

            migrationBuilder.CreateTable(
                name: "BenchmarkValidationResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BenchmarkResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Validator = table.Column<string>(type: "text", nullable: false),
                    ValidationType = table.Column<int>(type: "integer", nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    JudgeProvider = table.Column<string>(type: "text", nullable: true),
                    JudgeModel = table.Column<string>(type: "text", nullable: true),
                    JudgeInputTokens = table.Column<int>(type: "integer", nullable: true),
                    JudgeOutputTokens = table.Column<int>(type: "integer", nullable: true),
                    JudgePredictedInputTokens = table.Column<int>(type: "integer", nullable: true),
                    JudgeLatencyMs = table.Column<long>(type: "bigint", nullable: true),
                    JudgeEstimatedCost = table.Column<decimal>(type: "numeric", nullable: true),
                    RawJudgeResponse = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenchmarkValidationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BenchmarkValidationResults_BenchmarkResults_BenchmarkResult~",
                        column: x => x.BenchmarkResultId,
                        principalTable: "BenchmarkResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenchmarkValidationResults_BenchmarkResultId",
                table: "BenchmarkValidationResults",
                column: "BenchmarkResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenchmarkValidationResults");

            migrationBuilder.AddColumn<bool>(
                name: "LinksOk",
                table: "BenchmarkResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PlaceholdersOk",
                table: "BenchmarkResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SmsSegments",
                table: "BenchmarkResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
