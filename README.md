# LLMBenchmark

LLMBenchmark is a modular .NET benchmarking platform for evaluating Large Language Models (LLMs) in structured SMS generation and transformation workflows.

The platform focuses on benchmarking model quality, token estimation accuracy, latency, deterministic validation, and LLM-as-a-Judge evaluation across multiple providers and models.

---

# Main Features

* Multi-provider LLM benchmarking
* Scenario-driven execution
* Deterministic validators
* LLM-as-a-Judge evaluation
* Token estimation analysis
* Latency measurement
* Cost estimation
* SMS-specific validation
* Provider abstraction
* Extensible validation pipeline

---

# Goals

The platform aims to answer questions such as:

* Which LLM produces the best SMS outputs?
* Which model is the fastest?
* Which provider is the cheapest?
* Which model preserves placeholders correctly?
* Which tokenizer is more accurate?
* How accurate are token estimators?
* Which model follows instructions more reliably?
* Which model is safest against prompt injection?

---

# Architecture

```text id="f1z2x3"
Features/
 └── Benchmark/
      ├── Contracts/
      ├── Endpoints/
      ├── Enums/
      ├── Models/
      │    ├── Benchmark/
      │    ├── Estimator/
      │    └── Providers/
      ├── Providers/
      ├── Services/
      │    ├── Estimators/
      │    ├── Runner/
      │    └── Scenarios/
      └── Validators/
           ├── Deterministic/
           └── Judge/
```

---

# Benchmark Pipeline

```text id="m4n5b6"
Load Scenarios
      ↓
Build Request
      ↓
Estimate Tokens
      ↓
Execute Provider
      ↓
Measure Latency
      ↓
Persist Results
      ↓
Run Deterministic Validators
      ↓
Optional LLM-as-a-Judge Evaluation
      ↓
Store Validation Results
```

---

# Scenario-Driven Benchmarking

Benchmarks are driven by JSON scenarios.

Each scenario represents a structured SMS operation executed by the benchmark pipeline.

Example:

```json id="q7w8e9"
[
  {
    "id": "gen_ptpt_promo_casual_001",
    "action": "Generate",
    "input": {
      "prompt": "Cria um SMS promocional para clientes recorrentes da ModaMix com 30% desconto em toda a loja e inclui uma CTA para visitar o site hoje.",
      "language": "pt-PT",
      "tone": "casual"
    }
  }
]
```

---

# Scenario Structure

| Field    | Description                                        |
| -------- | -------------------------------------------------- |
| `id`     | Unique scenario identifier                         |
| `action` | SMS operation to execute                           |
| `input`  | Structured payload used during benchmark execution |

---

# Supported Actions

Current supported SMS actions:

| Action       | Description                         |
| ------------ | ----------------------------------- |
| `Generate`   | Generates a new SMS from a prompt   |
| `Rewrite`    | Rewrites an existing SMS            |
| `Shorten`    | Reduces SMS length                  |
| `Expand`     | Expands SMS content                 |
| `Formalize`  | Converts SMS to a formal tone       |
| `Casualize`  | Converts SMS to a casual tone       |
| `FixGrammar` | Corrects grammar and writing issues |

---

# Validation System

The validation pipeline is divided into two categories:

| Type                     | Purpose                  |
| ------------------------ | ------------------------ |
| Deterministic Validators | Exact rule validation    |
| LLM Judge Validators     | Subjective AI evaluation |

---

# Deterministic Validators

Current deterministic validators include:

* PlaceholderValidator
* LinkValidator
* CharacterLimitValidator
* SmsSegmentValidator
* CriticalInfoValidator

---

# LLM-as-a-Judge

The judge pipeline evaluates:

* Meaning preservation
* Tone adherence
* Language quality
* Instruction adherence
* SMS suitability
* Safety
* Prompt injection resistance

---

# Persistence

Main persisted entities:

* BenchmarkRun
* BenchmarkResult
* BenchmarkValidationResult

---

# Token Estimation

The platform supports multiple token estimation strategies for comparison and benchmarking purposes.

Current estimators:

* HeuristicTokenEstimator
* SharpTokenEstimator

The goal is to compare estimated token counts against provider-reported usage metrics and evaluate estimation accuracy across models.

---

# Providers

The architecture supports multiple providers through abstraction layers.

Current provider implementations:

* GitHub Models

The provider layer is designed to support future integrations such as:

* OpenAI
* Azure OpenAI
* Anthropic
* Google Gemini
* Mistral
* DeepSeek

---

# Tech Stack

* .NET 10
* ASP.NET Core Minimal API
* PostgreSQL
* Entity Framework Core
* Docker
* LlmTornado
* SharpToken

---

# Running Locally

## Requirements

* .NET 10 SDK
* Docker
* PostgreSQL

---

## Start PostgreSQL

```bash id="r2t3y4"
docker compose up -d
```

---

## Run Migrations

```bash id="u5i6o7"
dotnet ef database update
```

---

## Run API

```bash id="p8a9s0"
dotnet run --project LLMBenchmark.Api
```

---

# Configuration

Configuration is managed through:

* `appsettings.json`
* `appsettings.Development.json`
* Environment variables

Sensitive credentials should be stored using environment variables or secret managers.

---

# Future Improvements

Planned improvements include:

* Multi-provider execution
* Benchmark dashboard
* Cost reporting
* Parallel execution
* Retry policies
* Streaming support
* Prompt versioning
* Scenario tagging
* Benchmark comparison reports
* Batch execution
* Benchmark history visualization

---

# Development Status

This project is currently under active development.
