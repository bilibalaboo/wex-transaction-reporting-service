# Rule: Observability & Automated Testing
- **OpenTelemetry (OTel):** Deeply integrate OTel for Traces, Metrics, and Logs. Auto-instrument HTTP requests and EF Core queries.).
- **Logging:** Use Source Generated Logging (`LoggerMessage` attribute) for high-performance, zero-allocation structured logs.
- **Testing Framework:** xUnit + FluentAssertions. Follow the AAA (Arrange, Act, Assert) pattern.
- **Mocking:** Use `NSubstitute` or `Moq` to mock external dependencies (e.g., `HttpClient`).
- **Coverage Focus:** Prioritize unit testing for the 6-month lookback logic, Available Balance calculations, provider strategy routing, and Domain Entity invariants.