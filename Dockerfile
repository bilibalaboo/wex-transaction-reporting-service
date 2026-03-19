FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY ["src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj", "src/Wex.TransactionReporting.Api/"]
COPY ["src/Wex.TransactionReporting.Application/Wex.TransactionReporting.Application.csproj", "src/Wex.TransactionReporting.Application/"]
COPY ["src/Wex.TransactionReporting.Domain/Wex.TransactionReporting.Domain.csproj", "src/Wex.TransactionReporting.Domain/"]
COPY ["src/Wex.TransactionReporting.Infrastructure/Wex.TransactionReporting.Infrastructure.csproj", "src/Wex.TransactionReporting.Infrastructure/"]
COPY ["tests/Wex.TransactionReporting.Domain.Tests/Wex.TransactionReporting.Domain.Tests.csproj", "tests/Wex.TransactionReporting.Domain.Tests/"]
COPY ["tests/Wex.TransactionReporting.Application.Tests/Wex.TransactionReporting.Application.Tests.csproj", "tests/Wex.TransactionReporting.Application.Tests/"]

RUN dotnet restore "src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj" && \
    dotnet restore "tests/Wex.TransactionReporting.Domain.Tests/Wex.TransactionReporting.Domain.Tests.csproj" && \
    dotnet restore "tests/Wex.TransactionReporting.Application.Tests/Wex.TransactionReporting.Application.Tests.csproj"

COPY src/ src/
COPY tests/ tests/

# ── Test stage ────────────────────────────────────────────────────────────────
FROM restore AS test

# Fail the build if line coverage drops below 80%
RUN dotnet test tests/Wex.TransactionReporting.Domain.Tests/Wex.TransactionReporting.Domain.Tests.csproj \
        --no-restore \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=cobertura \
        /p:Threshold=80 \
        /p:ThresholdType=line \
        /p:ThresholdStat=total \
        '/p:Exclude=[*]*.Migrations.*' \
        '/p:ExcludeByFile=**/Program.cs' && \
    dotnet test tests/Wex.TransactionReporting.Application.Tests/Wex.TransactionReporting.Application.Tests.csproj \
        --no-restore \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=cobertura \
        /p:Threshold=80 \
        /p:ThresholdType=line \
        /p:ThresholdStat=total \
        '/p:Exclude=[*]*.Migrations.*' \
        '/p:ExcludeByFile=**/Program.cs'

# ── Build stage ───────────────────────────────────────────────────────────────
FROM test AS build

RUN dotnet publish "src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj" \
    -c Release \
    --no-restore \
    -o /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd -r appgroup && useradd -r -g appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Wex.TransactionReporting.Api.dll"]
