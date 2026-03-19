using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Errors;
using Wex.TransactionReporting.Infrastructure.Options;

namespace Wex.TransactionReporting.Infrastructure.ExchangeRates;

public sealed class TreasuryExchangeRateService(
    IHttpClientFactory httpClientFactory,
    IOptions<TreasuryApiOptions> options) : IExchangeRateService
{
    private const string Fields = "country_currency_desc,exchange_rate,effective_date";

    private HttpClient CreateClient() =>
        httpClientFactory.CreateClient(nameof(TreasuryExchangeRateService));

    public async Task<Result<ExchangeRateResult>> GetForTransactionDateAsync(
        string currency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default)
    {
        var windowStart = transactionDate.AddMonths(-6);

        var url = BuildUrl(options.Value.BaseUrl,
            $"country_currency_desc:eq:{Uri.EscapeDataString(currency)}," +
            $"effective_date:lte:{transactionDate:yyyy-MM-dd}," +
            $"effective_date:gte:{windowStart:yyyy-MM-dd}");

        return await FetchLatestRate(url, cancellationToken);
    }

    public async Task<Result<ExchangeRateResult>> GetLatestAsync(
        string currency,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(options.Value.BaseUrl,
            $"country_currency_desc:eq:{Uri.EscapeDataString(currency)}");

        return await FetchLatestRate(url, cancellationToken);
    }

    private async Task<Result<ExchangeRateResult>> FetchLatestRate(
        string url,
        CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var apiResponse = await JsonSerializer.DeserializeAsync(
            stream,
            InfrastructureJsonContext.Default.TreasuryApiResponse,
            cancellationToken);

        var entry = apiResponse?.Data.FirstOrDefault();
        if (entry is null)
            return DomainErrors.ExchangeRate.NotFound;

        return new ExchangeRateResult(
            Currency: entry.CountryCurrencyDesc,
            EffectiveDate: DateOnly.Parse(entry.EffectiveDate, CultureInfo.InvariantCulture),
            Rate: decimal.Parse(entry.ExchangeRate, CultureInfo.InvariantCulture));
    }

    private static string BuildUrl(string baseUrl, string filter) =>
        $"{baseUrl}?fields={Fields}&filter={filter}&sort=-effective_date&page[size]=1";
}
