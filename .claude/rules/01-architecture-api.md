# Rule: Architecture & API Design
- **Clean Architecture:** Strictly separate Domain, Application, Infrastructure, and API layers.
- **Minimal APIs:** Use Minimal APIs (`MapGet`, `MapPost`, `MapGroup`) for the presentation layer instead of traditional MVC Controllers. Use extension methods to logically group endpoint registrations.
- **Native AOT Compatibility:** The application must be fully compatible with Native AOT compilation. **Strictly avoid reflection** in the business logic and API layers.
- **JSON Serialization:** Must use `System.Text.Json` with Source Generators (`JsonSerializerContext`) for all API requests/responses to ensure Native AOT compatibility and zero-reflection serialization.
- **CQRS:** Segregate read and write operations.
- **Result Pattern:** Implement the `Result<T>` pattern for all application flow control and error handling. Do not use exceptions for business logic flow.
- **Request Validation:** Use `FluentValidation`. Intercept and validate incoming HTTP requests using Minimal API Endpoint Filters before they reach the Application layer.
- **Configuration Validation:** Use .NET's native `[OptionsValidator]` with Source Generators to validate `appsettings.json` bindings at application startup to enforce a Fail-Fast mechanism.
- **Idempotency:** Implement idempotency for `POST /transactions` using an `Idempotency-Key` header to prevent duplicate processing.