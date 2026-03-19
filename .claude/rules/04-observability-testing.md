# Rule: Observability & Automated Testing
- **OpenTelemetry (OTel):** Deeply integrate OTel for Traces, Metrics, and Logs. Auto-instrument HTTP requests and EF Core queries.
- **Distributed Tracing:** Use a custom `ActivitySource` to create spans for business operations (e.g., Treasury API calls). Spans are automatically parented to the incoming W3C TraceContext, chaining traces across microservices.
- **Metrics:** Use a custom `Meter` with meaningful instruments (Counter, Histogram) to track business and performance signals (e.g., Treasury API request duration, cache hit/miss rate, transactions created). These feed into alerting (e.g., alert if p95 Treasury API duration > 5s).
- **Logging:** Use Serilog as the logging provider, integrated with the OTel log pipeline via `Serilog.Sinks.OpenTelemetry`. Use Source Generated Logging (`LoggerMessage` attribute) for high-performance, zero-allocation structured logs.
- **Testing Framework:** xUnit + FluentAssertions. Follow the AAA (Arrange, Act, Assert) pattern.
- **Mocking:** Use `NSubstitute` to mock external dependencies (e.g., `IExchangeRateService`, `ICardRepository`).
- **Coverage Focus:** Prioritize unit testing for the 6-month lookback logic, Available Balance calculations, exchange rate conversion, and Domain Entity invariants.
