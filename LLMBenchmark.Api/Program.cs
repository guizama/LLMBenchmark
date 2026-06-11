using DotNetEnv;
using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Endpoints;
using LLMBenchmark.Api.Features.Benchmark.Providers;
using LLMBenchmark.Api.Features.Benchmark.Services.Estimators;
using LLMBenchmark.Api.Features.Benchmark.Services.Runner;
using LLMBenchmark.Api.Features.Benchmark.Services.Scenarios;
using LLMBenchmark.Api.Features.Benchmark.Validators.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Deterministic;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Services;
using LLMBenchmark.Api.Persistence;
using LlmTornado;
using LlmTornado.Code;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"));
});

builder.Services.AddScoped<ScenarioLoader>();
builder.Services.Configure<GitHubModelsOptions>(builder.Configuration.GetSection("GitHubModels"));

builder.Services.AddSingleton(new TornadoApi([
    new ProviderAuthentication(
        LLmProviders.OpenAi,
        builder.Configuration["GitHubModels:Token"]!
    )
]));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

builder.Services.AddScoped<ILLMProvider, GitHubModelsProvider>();
builder.Services.AddScoped<HeuristicTokenEstimator>();
builder.Services.AddScoped<SharpTokenEstimator>();

builder.Services.AddScoped<ITokenEstimatorFactory, TokenEstimatorFactory>();

builder.Services.AddScoped<IBenchmarkValidator, PlaceholderValidator>();
builder.Services.AddScoped<IBenchmarkValidator, LinkValidator>();
builder.Services.AddScoped<IBenchmarkValidator, CharacterLimitValidator>();
builder.Services.AddScoped<IBenchmarkValidator, SmsSegmentValidator>();
builder.Services.AddScoped<IBenchmarkValidator, CriticalInfoValidator>();

builder.Services.AddScoped<JudgeDecisionService>();
builder.Services.AddScoped<ILLMJudgeService, LLMJudgeService>();
builder.Services.AddScoped<IBenchmarkValidator, ScenarioJudgeValidator>();

builder.Services.AddScoped<BenchmarkRunner>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapBenchmarkEndpoints();

app.Run();