FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj", "src/Wex.TransactionReporting.Api/"]
COPY ["src/Wex.TransactionReporting.Application/Wex.TransactionReporting.Application.csproj", "src/Wex.TransactionReporting.Application/"]
COPY ["src/Wex.TransactionReporting.Domain/Wex.TransactionReporting.Domain.csproj", "src/Wex.TransactionReporting.Domain/"]
COPY ["src/Wex.TransactionReporting.Infrastructure/Wex.TransactionReporting.Infrastructure.csproj", "src/Wex.TransactionReporting.Infrastructure/"]

RUN dotnet restore "src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj"

COPY src/ src/

RUN dotnet publish "src/Wex.TransactionReporting.Api/Wex.TransactionReporting.Api.csproj" \
    -c Release \
    --no-restore \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd -r appgroup && useradd -r -g appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Wex.TransactionReporting.Api.dll"]
