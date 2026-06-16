# LLMBenchmark.Api

LLMBenchmark.Api is a .NET Minimal API project designed to benchmark, evaluate, and validate Large Language Models (LLMs) for SMS generation and SMS transformation tasks.

The project focuses on:

- Multi-model benchmarking
- Token estimation accuracy
- Latency measurement
- SMS optimization
- Deterministic validation
- LLM-as-a-Judge evaluation
- Cost analysis
- Provider abstraction
- Scenario-driven testing

The system aims to become a production-grade benchmark and evaluation platform for AI-powered SMS generation systems.

## Goals

This project aims to answer questions such as:

- Which LLM performs best for SMS generation?
- Which model is cheaper?
- Which model preserves meaning better?
- Which model follows SMS constraints more accurately?
- Which tokenizer is more accurate?
- How accurate are token estimators?
- Which model produces the best output quality?
- Which model has the best latency/cost balance?
- Which validator fails most often?
- Which provider has the best cost/performance ratio?

## Current Architecture

The project follows a modular architecture organized by feature.

```
Features/
 └── Benchmark/
      ├── Contracts/
      ├── Endpoints/
      ├── Enums/
      ├── Models/
      ├── Providers/
      ├── Services/
      │    ├── Estimators/
      │    ├── Runner/
      │    └── Scenarios/
      └── Validators/
           ├── Contracts/
           ├── Deterministic/
           ├── Judge/
           └── Models/
```

## Core Concepts

### Scenario-Driven Benchmarking

Benchmarks are fully driven by JSON scenarios.

**Example:**

```json
[
  {
    "id": "gen_ptpt_promo_casual_001",
    "action": "sms.generate",
    "input": {
      "prompt": "Cria um SMS promocional para clientes recorrentes da ModaMix com 30% desconto em toda a loja e inclui uma CTA para visitar o site hoje.",
      "language": "pt-PT",
      "tone": "casual"
    }
  }
]
```

### Supported SMS Actions

```csharp
public enum SmsAction
{
    Generate,
    Rewrite,
    Shorten,
    Expand,
    Formalize,
    Casualize,
    FixGrammar
}
```

### Benchmark Pipeline

```
Load Scenarios
      ↓
Build Prompt
      ↓
Estimate Tokens
      ↓
Execute Provider
      ↓
Measure Latency
      ↓
Persist Results
      ↓
Run Validators
      ↓
Optional LLM-as-a-Judge
      ↓
Store Validation Results
```

## Validation System

Validators are split into 2 categories:

| Type | Purpose |
|------|---------|
| Deterministic | Exact validation |
| LLM Judge | Subjective AI evaluation |

### Deterministic Validators

- PlaceholderValidator
- LinkValidator
- CharacterLimitValidator
- SmsSegmentValidator
- CriticalInfoValidator

### LLM-as-a-Judge

Current judge model:

**GPT-4.1 Mini**

Judge evaluates:

- Meaning preservation
- Tone adherence
- Language quality
- Instruction adherence
- SMS suitability
- Safety
- Prompt injection resistance

## Persistence

Main entities:

- BenchmarkRun
- BenchmarkResult
- BenchmarkValidationResult

## Tech Stack

- .NET 10
- ASP.NET Core Minimal API
- PostgreSQL
- Entity Framework Core
- LlmTornado
- SharpToken

## Running the Project

### Requirements

Install:

- .NET 10 SDK
- Docker Desktop

### Start PostgreSQL

The project includes a docker-compose configuration for PostgreSQL.

Run:

```bash
docker compose up -d
```

This starts the PostgreSQL container used by the benchmark API.

### Run Migrations

```bash
dotnet ef database update --project LLMBenchmark.Api
```

### Run the API

```bash
dotnet run --project LLMBenchmark.Api
```

Default API URL:

- `http://localhost:5000`
- `https://localhost:5001`

(depending on local ASP.NET configuration)

## Dashboard

The dashboard is fully static and reads local JSON files generated from benchmark query exports served by nginx.

Run:

```bash
docker run --rm -it -p 8080:80 -v ${PWD}:/usr/share/nginx/html nginx
```

Dashboard URL:

- `http://localhost:8080`

### Dashboard Data Files

The dashboard expects JSON files generated from PostgreSQL queries.

Place these files in the dashboard root directory:

- `benchmark-run.json`
- `benchmark-results.json`
- `validators-breakdown.json`

### Exporting Dashboard Data

You must export PostgreSQL query results as JSON files.

Example using DBeaver:

1. Run query
2. Export Resultset
3. Format: JSON
4. Save with expected filename

### PostgreSQL Export Queries

#### benchmark-run.json

```sql
SELECT
    "Id", "StartedAtUtc", 
    "FinishedAtUtc",
    EXTRACT( EPOCH FROM ( "FinishedAtUtc" - "StartedAtUtc")) AS "DurationSeconds",
    "Status", 
    "TotalScenarios", 
    "TotalExecutions", 
    "SuccessCount", 
    "FailureCount"
FROM public."BenchmarkRuns"
ORDER BY "StartedAtUtc" DESC
```

#### benchmark-results.json

```sql
SELECT
    r."Id" AS "ResultId", 
    r."Timestamp", 
    r."ScenarioId", 
    r."Provider", 
    r."Model",
    CONCAT(r."Provider", ' / ', r."Model") AS "ProviderModel",
    r."Action", 
    r."Language", 
    r."Success",
    -- TOKENS
    r."InputTokens", 
    r."OutputTokens",
    (
        COALESCE(r."InputTokens",0)
        + COALESCE(r."OutputTokens",0)
    ) AS "TotalTokens",
    r."PredictedInputTokens", 
    r."InputTokenDelta", 
    r."InputTokenErrorPercent",
    -- OUTPUT
    r."OutputCharacters", 
    r."OutputEstimatedSmsSegmentsQtd",
    -- LATENCY
    r."EndToEndLatencyMs", 
    r."ProviderLatencyMs",
    -- CONFIG
    r."Temperature", 
    r."TokenEstimator",
    -- MAIN JUDGE SCORE
    MAX(
        CASE
            WHEN vr."Validator" = 'ScenarioJudgeValidator'
            THEN vr."Score"
        END
    ) AS "JudgeScore",
    -- VALIDATIONS
    COUNT(*) FILTER (
        WHERE vr."Passed" = true
    ) AS "PassedValidations",
    COUNT(*) FILTER (
        WHERE vr."Passed" = false
    ) AS "FailedValidations",
    -- TOKEN EFFICIENCY
    CASE
        WHEN (
            COALESCE(r."InputTokens",0)
            + COALESCE(r."OutputTokens",0)
        ) > 0
        THEN
            MAX(
                CASE
                    WHEN vr."Validator" = 'ScenarioJudgeValidator'
                    THEN vr."Score"
                END
            )
            /
            (
                COALESCE(r."InputTokens",0)
                + COALESCE(r."OutputTokens",0)
            )
    END AS "ScorePerToken"
FROM public."BenchmarkResults" r
LEFT JOIN public."BenchmarkValidationResults" vr ON vr."BenchmarkResultId" = r."Id"
GROUP BY r."Id", r."Timestamp", r."ScenarioId", r."Provider", r."Model", r."Action", r."Language", r."Success", r."InputTokens", r."OutputTokens", r."PredictedInputTokens",
        r."InputTokenDelta", r."InputTokenErrorPercent", r."OutputCharacters", r."OutputEstimatedSmsSegmentsQtd", r."EndToEndLatencyMs", r."ProviderLatencyMs", r."Temperature",
        r."TokenEstimator"
ORDER BY r."Timestamp" DESC
```

#### validators-breakdown.json

```sql
SELECT
    r."Provider", 
    r."Model", 
    r."Action", 
    vr."Validator",
    COUNT(*) AS "Total",
    COUNT(*) FILTER (
        WHERE vr."Passed" = true
    ) AS "Passed",
    COUNT(*) FILTER (
        WHERE vr."Passed" = false
    ) AS "Failed",
    AVG(vr."Score") AS "AverageScore"
FROM public."BenchmarkResults" r
INNER JOIN public."BenchmarkValidationResults" vr ON vr."BenchmarkResultId" = r."Id"
GROUP BY r."Provider", r."Model", r."Action", vr."Validator"
ORDER BY r."Model", vr."Validator"
```

## Example Workflow

### 1. Start PostgreSQL

```bash
docker compose up -d
```

### 2. Run migrations

```bash
dotnet ef database update --project LLMBenchmark.Api
```

### 3. Start API

```bash
dotnet run --project LLMBenchmark.Api
```

### 4. Execute benchmarks

Use the endpoints directly or execute requests from:

- `LLMBenchmark.Api.http`

### 5. Export dashboard JSON files

Generate:

- `benchmark-run.json`
- `benchmark-results.json`
- `validators-breakdown.json`

### 6. Start dashboard

```bash
docker run --rm -it -p 8080:80 -v ${PWD}:/usr/share/nginx/html nginx
```

### 7. Open dashboard

- `http://localhost:8080`

## Future Improvements

Planned features:

- Multi-provider execution
- Parallel benchmark orchestration
- OpenRouter integration
- LiteLLM gateway support
- Cost-per-quality ranking
- Automatic leaderboard generation
- Prompt versioning
- Historical benchmark comparison
- Real tokenizer calibration
- Benchmark replay support
- Multi-language evaluation datasets
- Web dashboard with live metrics
- Distributed benchmark runners
- Automatic report generation
- Benchmark diff analysis