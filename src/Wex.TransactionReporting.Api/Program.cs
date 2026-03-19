using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using Wex.TransactionReporting.Api.Endpoints;
using Wex.TransactionReporting.Api.Models;
using Wex.TransactionReporting.Api.Serialization;
using Wex.TransactionReporting.Application.Cards.Commands.CreateCard;
using Wex.TransactionReporting.Application.Cards.Queries.GetCardBalance;
using Wex.TransactionReporting.Application.Transactions.Commands.StoreTransaction;
using Wex.TransactionReporting.Application.Transactions.Queries.GetTransactionInCurrency;
using Wex.TransactionReporting.Infrastructure;
using Wex.TransactionReporting.Infrastructure.Persistence;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog(static (context, config) =>
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.OpenTelemetry(opts =>
        {
            opts.Endpoint = context.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
            opts.Protocol = OtlpProtocol.Grpc;
            opts.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = "Wex.TransactionReporting"
            };
        }));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<CreateCardCommandHandler>();
builder.Services.AddScoped<GetCardBalanceQueryHandler>();
builder.Services.AddScoped<StoreTransactionCommandHandler>();
builder.Services.AddScoped<GetTransactionInCurrencyQueryHandler>();

builder.Services.AddScoped<IValidator<CreateCardCommand>, CreateCardCommandValidator>();
builder.Services.AddScoped<IValidator<StoreTransactionRequest>, StoreTransactionRequestValidator>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Wex.TransactionReporting"))
    .WithTracing(tracing => tracing
        .AddInfrastructureTracing()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddInfrastructureMetrics()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapCardEndpoints();
app.MapTransactionEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
#pragma warning disable IL3050
    await db.Database.MigrateAsync();
#pragma warning restore IL3050
}

app.Run();
