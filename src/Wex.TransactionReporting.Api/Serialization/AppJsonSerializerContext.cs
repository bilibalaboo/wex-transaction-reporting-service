using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Wex.TransactionReporting.Api.Models;
using Wex.TransactionReporting.Application.Cards.Commands.CreateCard;
using Wex.TransactionReporting.Application.Cards.Queries.GetCardBalance;
using Wex.TransactionReporting.Application.Transactions.Queries.GetTransactionInCurrency;

namespace Wex.TransactionReporting.Api.Serialization;

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(CreateCardCommand))]
[JsonSerializable(typeof(StoreTransactionRequest))]
[JsonSerializable(typeof(CardBalanceResponse))]
[JsonSerializable(typeof(TransactionInCurrencyResponse))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
