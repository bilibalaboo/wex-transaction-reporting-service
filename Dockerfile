FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj", "src/Wex.TransactionReporting.Api/"]
COPY ["src/Wex.TransactionReporting.Application/Wex.TransactionReporting.Application.csproj", "src/Wex.TransactionReporting.Application/"]
COPY ["src/Wex.TransactionReporting.Domain/Wex.TransactionReporting.Domain.csproj", "src/Wex.TransactionReporting.Domain/"]
COPY ["src/Wex.TransactionReporting.Infrastructure/Wex.TransactionReporting.Infrastructure.csproj", "src/Wex.TransactionReporting.Infrastructure/"]

RUN dotnet restore "src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj" -r linux-musl-x64

COPY src/ src/

RUN dotnet publish "src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj" \
    -c Release \
    -r linux-musl-x64 \
    --no-restore \
    /p:PublishAot=true \
    /p:StripSymbols=true \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-preview-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["./Wex.TransactionReporting.Api"]
