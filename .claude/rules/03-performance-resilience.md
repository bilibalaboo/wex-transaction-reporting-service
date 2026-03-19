# Rule: Performance & External Resilience
- **Memory Management:** Use `Span<T>` and `ReadOnlySpan<T>` for parsing and buffers. Use `scoped ref struct` for high-performance wrappers to ensure stack-only allocation.
- **Allocations:** Minimize heap allocations in hot paths. Use `stackalloc` where appropriate and strictly avoid closures.
- **Asynchrony:** Use `ValueTask` or `ValueTask<T>` for methods likely to complete synchronously.
- **Language Features:** Heavily utilize `params collections` and `primary constructors` (C# 14 / .NET 10).
- **AOT-Friendly DI:** Ensure all Dependency Injection registrations are explicit and AOT-friendly (avoid assembly scanning via reflection).
- **ResiliencePipeline:** Use Polly v8 non-generic `ResiliencePipeline` for all HTTP calls to external rate providers (Treasury/ATO APIs). 
- **Strategies:** Implement Exponential backoff with jitter for retries (5xx, 429), a Circuit Breaker (break after 3 failures), and a strict 10-second total Timeout.
- **Distributed Caching:** Implement a **Cache-Aside Pattern** using `IDistributedCache` (backed by Redis) for external API exchange rates. Use a structured key like `Provider_Currency_Date` to minimize latency, prevent cache stampedes, and ensure data consistency across multiple API instances.