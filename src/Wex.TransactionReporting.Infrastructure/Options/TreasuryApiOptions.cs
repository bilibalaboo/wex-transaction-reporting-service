using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Wex.TransactionReporting.Infrastructure.Options;

public sealed class TreasuryApiOptions
{
    public const string SectionName = "TreasuryApi";

    [Required, Url]
    public string BaseUrl { get; init; } = string.Empty;
}

[OptionsValidator]
public partial class TreasuryApiOptionsValidator : IValidateOptions<TreasuryApiOptions> { }
