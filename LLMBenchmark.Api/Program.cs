using DotNetEnv;
using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Judge;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Provider;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Validator;
using LLMBenchmark.Api.Features.Benchmark.Endpoints;
using LLMBenchmark.Api.Features.Benchmark.Services.Estimators;
using LLMBenchmark.Api.Features.Benchmark.Services.Providers;
using LLMBenchmark.Api.Features.Benchmark.Services.Runner;
using LLMBenchmark.Api.Features.Benchmark.Services.Scenarios;
using LLMBenchmark.Api.Features.Benchmark.Validators.Deterministic;
using LLMBenchmark.Api.Features.Benchmark.Validators.Input;
using LLMBenchmark.Api.Features.Benchmark.Validators.Output.Judge;
using LLMBenchmark.Api.Features.Benchmark.Validators.Output.Judge.Services;
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
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"));
});

builder.Services.AddScoped<ScenarioLoader>();

builder.Services.Configure<LLMProviderOptions>("GitHubModels", builder.Configuration.GetSection("Providers:GitHubModels"));
builder.Services.Configure<LLMProviderOptions>("OpenAI", builder.Configuration.GetSection("Providers:OpenAI"));
builder.Services.Configure<LLMProviderOptions>("Anthropic", builder.Configuration.GetSection("Providers:Anthropic"));

builder.Services.AddSingleton(new TornadoApi(
[
    new ProviderAuthentication(LLmProviders.OpenAi, builder.Configuration["Providers:OpenAI:ApiKey"]!),
    new ProviderAuthentication(LLmProviders.Custom, builder.Configuration["Providers:GitHubModels:ApiKey"]!),
    new ProviderAuthentication(LLmProviders.Anthropic, builder.Configuration["Providers:Anthropic:ApiKey"]!)
]));


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen(options =>
{
    options.UseInlineDefinitionsForEnums();
});

builder.Services.AddScoped<ILLMProvider, GitHubModelsProvider>();
builder.Services.AddScoped<ILLMProvider, OpenAIProvider>();
builder.Services.AddScoped<ILLMProvider, AnthropicProvider>();

builder.Services.AddScoped<HeuristicTokenEstimator>();
builder.Services.AddScoped<SharpTokenEstimator>();
builder.Services.AddScoped<AnthropicApiTokenEstimator>();
builder.Services.AddScoped<ITokenEstimatorFactory, TokenEstimatorFactory>();

builder.Services.AddScoped<IBenchmarkValidator, PlaceholderValidator>();
builder.Services.AddScoped<IBenchmarkValidator, LinkValidator>();
builder.Services.AddScoped<IBenchmarkValidator, CriticalInfoValidator>();

builder.Services.AddScoped<JudgeDecisionService>();
builder.Services.AddScoped<ILLMJudgeService, LLMJudgeService>();
builder.Services.AddScoped<IBenchmarkValidator, ScenarioJudgeValidator>();

builder.Services.AddScoped<ScenarioRequestValidator>();
builder.Services.AddScoped<BenchmarkRunner>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapBenchmarkEndpoints();
app.Run();