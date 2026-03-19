using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Wex.TransactionReporting.Infrastructure.Observability;

[ExcludeFromCodeCoverage]
public static class AppActivitySource
{
    public const string Name = "Wex.TransactionReporting";
    public static readonly ActivitySource Instance = new(Name, "1.0.0");
}
