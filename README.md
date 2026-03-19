# Wex Transaction Reporting API

A production-ready .NET 10 REST API for card transaction management and currency conversion using the US Treasury Rates of Exchange.

---

## AI Disclosure

This project was built in collaboration with **Claude (Anthropic)** as an AI pair-programming assistant.

My role throughout the process:
- **Solution Architect** — defined the overall structure, layer boundaries, and key design decisions (Clean Architecture, CQRS without MediatR, rich domain model with SQL-side aggregation, Cache-Aside pattern)
- **Technical Supervisor** — guided trade-off discussions (ValueTask vs Task, typed vs named HttpClient, decorator pattern, Specification pattern), and made the final call on each
- **Code Reviewer** — reviewed every file generated, identified issues (incorrect API URL, wrong HTTP status codes, missing `.dockerignore`, AOT/trimming incompatibilities), and directed fixes
- **QA** — ran the end-to-end test script, debugged failures, and confirmed all 14 test cases pass against live infrastructure

Claude generated the majority of the implementation code under my direction. All architectural decisions, design reviews, bug identification, and quality gates were driven by me.

---

## Requirements

- Docker Desktop
- .NET 10 SDK (for running tests locally)

---

## Running the Application

```bash
docker compose up --build
```

This will:
1. Start PostgreSQL 17 and Redis
2. Build and start the API (waits for both dependencies to be healthy)
3. Apply EF Core migrations automatically on startup

The API is available at `http://localhost:8080`.

Interactive API documentation (Scalar UI): `http://localhost:8080/scalar/v1`

---

## Architecture

Clean Architecture with four layers:

```
Wex.TransactionReporting.Domain          — Entities, domain errors, repository interfaces
Wex.TransactionReporting.Application    — CQRS handlers, abstractions (IExchangeRateService)
Wex.TransactionReporting.Infrastructure — EF Core, Redis cache, Treasury API client
Wex.TransactionReporting.Api            — Minimal API endpoints, filters, serialization
```

### Key Design Decisions

| Decision | Choice | Reason |
|---|---|---|
| CQRS | Direct handler classes | No MediatR — avoids reflection, simpler DI |
| Domain model | Rich behaviour, no child collections | `Card.RecordTransaction()` owns validation; balance computed via SQL SUM |
| Exchange rate caching | Redis Cache-Aside | 6h TTL for historical rates, 15m for latest |
| HTTP resilience | Polly v8 via `Microsoft.Extensions.Http.Resilience` | Retry (exponential + jitter), circuit breaker, 10s timeout |
| Serialization | STJ Source Generators (`JsonSerializerContext`) | AOT-friendly, zero reflection |
| Validation | FluentValidation + Endpoint Filters | Validated before reaching application layer |
| Configuration | `[OptionsValidator]` source generators | Fail-fast on startup if config is missing |
| Logging | Serilog + `Serilog.Sinks.OpenTelemetry` | Structured logs forwarded to OTel pipeline |
| Tracing | Custom `ActivitySource` | Spans for Treasury API calls, chained via W3C TraceContext |
| Metrics | Custom `Meter` | Treasury API duration histogram, cache hit/miss counters, transaction counters |

---

## Endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/cards` | Create a card with a credit limit |
| `POST` | `/cards/{cardId}/transactions` | Store a purchase transaction (requires `Idempotency-Key` header) |
| `GET` | `/cards/{cardId}/balance?currency=` | Get available balance converted to target currency (latest rate) |
| `GET` | `/transactions/{transactionId}?currency=` | Get transaction converted to target currency (rate on or before transaction date, within 6-month window) |

### Currency Values

The `currency` parameter must match the US Treasury `country_currency_desc` field exactly. Examples:

| Currency | Value |
|---|---|
| Euro | `Euro Zone-Euro` |
| British Pound | `United Kingdom-Pound` |
| Canadian Dollar | `Canada-Dollar` |
| Japanese Yen | `Japan-Yen` |
| Australian Dollar | `Australia-Dollar` |
| Mexican Peso | `Mexico-Peso` |

Browse all valid values: `https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange?fields=country_currency_desc&sort=country_currency_desc&page[size]=100`

---

## Running Tests

### Unit Tests

```bash
dotnet test
```

Covers domain entity invariants, all application handler scenarios (happy path, not found, invalid input, exchange rate failure), and idempotency behaviour. Uses xUnit, FluentAssertions, and NSubstitute.

### End-to-End Tests

Requires the Docker stack to be running.

```bash
powershell -ExecutionPolicy Bypass -File test-api.ps1
```

Runs 14 test cases against the live API:
- Card creation and validation
- Transaction storage, validation, and idempotency
- Currency conversion with live Treasury rates
- 6-month lookback window enforcement
- All 404 / 422 error cases

---

## Project Structure

```
src/
  Wex.TransactionReporting.Domain/
  Wex.TransactionReporting.Application/
  Wex.TransactionReporting.Infrastructure/
  Wex.TransactionReporting.Api/
tests/
  Wex.TransactionReporting.Domain.Tests/
  Wex.TransactionReporting.Application.Tests/
Dockerfile
docker-compose.yml
test-api.ps1
```
