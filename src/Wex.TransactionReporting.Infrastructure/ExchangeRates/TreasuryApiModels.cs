using System.Text.Json.Serialization;
using Wex.TransactionReporting.Application.Abstractions;

namespace Wex.TransactionReporting.Infrastructure.ExchangeRates;

internal sealed class TreasuryApiResponse
{
    [JsonPropertyName("data")]
    public List<TreasuryRateEntry> Data { get; init; } = [];
}

internal sealed class TreasuryRateEntry
{
    [JsonPropertyName("country_currency_desc")]
    public string CountryCurrencyDesc { get; init; } = string.Empty;

    [JsonPropertyName("exchange_rate")]
    public string ExchangeRate { get; init; } = string.Empty;

    [JsonPropertyName("record_date")]
    public string EffectiveDate { get; init; } = string.Empty;
}

[JsonSerializable(typeof(TreasuryApiResponse))]
[JsonSerializable(typeof(ExchangeRateResult))]
internal partial class InfrastructureJsonContext : JsonSerializerContext { }
