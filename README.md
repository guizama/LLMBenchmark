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

The system is being built as a production-grade benchmark and evaluation platform for AI-powered SMS generation systems.

---

# Goals

This project aims to answer questions such as:

- Which LLM performs best for generation?
- Which model is cheaper?
- Which model preserves placeholders correctly?
- Which model follows constraints better?
- Which tokenizer is more accurate?
- How accurate are token estimators?
- Which model produces the best output?
- Which model preserves meaning better?
- Which model is safest against prompt injection?

---

# Current Architecture

The project follows a modular architecture organized by feature.

```text
Features/
 └── Benchmark/
      ├── Contracts/
      ├── Endpoints/
      ├── Enums/
      ├── Estimators/
      ├── Models/
      ├── Providers/
      ├── Services/
      └── Validators/
```

---

# Core Concepts

## Scenario-Driven Benchmarking

Benchmarks are driven by JSON scenarios.

Example:

```json
{
  "id": "gen_ptpt_promo_casual_001",
  "category": "generation",
  "language": "PT-PT",
  "tone": "casual",
  "max_characters": 160,
  "expected_sms_segments": 1,
  "requirements": [
    "Must be concise",
    "Must include CTA"
  ],
  "prompt": "ModaMix: 30% desconto em toda a loja para clientes recorrentes. Visita o site hoje."
}
```

---

# Benchmark Pipeline

```text
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
Run Validators
      ↓
Optional LLM-as-a-Judge
      ↓
Store Validation Results
```

---

# Validation System

Validators are split into 2 categories:

| Type | Purpose |
|---|---|
| Deterministic | Exact validation |
| LLM Judge | Subjective AI evaluation |

---

# Deterministic Validators

- PlaceholderValidator
- LinkValidator
- CharacterLimitValidator
- SmsSegmentValidator
- CriticalInfoValidator

---

# LLM-as-a-Judge

Current judge model:

- GPT-4.1 Mini

Judge evaluates:

- Meaning preservation
- Tone adherence
- Language quality
- Instruction adherence
- SMS suitability
- Safety
- InjectionValidator

---

# Persistence

Main entities:

- BenchmarkRun
- BenchmarkResult
- BenchmarkValidationResult

---

# Tech Stack

- .NET 10
- ASP.NET Core Minimal API
- PostgreSQL
- Entity Framework Core
- LlmTornado
- SharpToken
