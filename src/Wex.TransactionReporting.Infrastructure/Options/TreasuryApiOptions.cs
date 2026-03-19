using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Wex.TransactionReporting.Infrastructure.Options;

[ExcludeFromCodeCoverage]
public sealed class TreasuryApiOptions
{
    public const string SectionName = "TreasuryApi";

    [Required, Url]
    public string BaseUrl { get; init; } = string.Empty;
}

[ExcludeFromCodeCoverage]
[OptionsValidator]
public partial class TreasuryApiOptionsValidator : IValidateOptions<TreasuryApiOptions> { }
