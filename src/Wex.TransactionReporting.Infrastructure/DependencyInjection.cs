using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Polly;
using System.Net;
using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Domain.Repositories;
using Wex.TransactionReporting.Infrastructure.ExchangeRates;
using Wex.TransactionReporting.Infrastructure.Observability;
using Wex.TransactionReporting.Infrastructure.Options;
using Wex.TransactionReporting.Infrastructure.Persistence;
using Wex.TransactionReporting.Infrastructure.Persistence.Repositories;

namespace Wex.TransactionReporting.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Redis"));

        services
            .AddOptions<TreasuryApiOptions>()
            .BindConfiguration(TreasuryApiOptions.SectionName)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<TreasuryApiOptions>, TreasuryApiOptionsValidator>();

        services.AddHttpClient(nameof(TreasuryExchangeRateService), static (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<TreasuryApiOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
        .AddResilienceHandler("treasury", static builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(10));

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is not null ||
                    args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests ||
                    (int?)args.Outcome.Result?.StatusCode >= 500)
            });

            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                MinimumThroughput = 3,
                FailureRatio = 1.0,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30)
            });
        });

        services.AddScoped<TreasuryExchangeRateService>();
        services.AddScoped<IExchangeRateService, CachedExchangeRateService>();

        return services;
    }

    public static TracerProviderBuilder AddInfrastructureTracing(this TracerProviderBuilder builder) =>
        builder.AddSource(AppActivitySource.Name);

    public static MeterProviderBuilder AddInfrastructureMetrics(this MeterProviderBuilder builder) =>
        builder.AddMeter(AppMeter.Name);
}
