# Rule: Persistence & Containerization
- **ORM:** Entity Framework Core. 
- **AOT Data Access Strategy:** Because EF Core has limitations with Native AOT, the AI must implement **EF Core Compiled Models** to ensure AOT compatibility. If full AOT is unachievable with the specific EF Core setup, fallback to `PublishTrimmed=true` and `EnableConfigurationBindingGenerator=true` in the project file.
- **Containerization:** The reviewer should only need Docker and the .NET SDK to run the application.
- **Database:** Use `PostgreSQL 17-alpine` for a lightweight footprint.
- **Docker Compose:** Use Named Volumes for Postgres data persistence. The API container must have a `DependsOn` configuration with a health check, waiting for Postgres to be fully ready before starting.
- **Dockerfile:** Use a multi-stage build. The publish step must include `/p:PublishAot=true` to compile the application to native code, resulting in a highly optimized, self-contained alpine-based production image.