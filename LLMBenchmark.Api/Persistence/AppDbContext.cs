using Microsoft.EntityFrameworkCore;

namespace LLMBenchmark.Api.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BenchmarkResult> BenchmarkResults => Set<BenchmarkResult>();
    public DbSet<BenchmarkRun> BenchmarkRuns => Set<BenchmarkRun>();
    public DbSet<BenchmarkValidationResult> BenchmarkValidationResults => Set<BenchmarkValidationResult>();
}