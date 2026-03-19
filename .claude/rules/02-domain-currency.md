# Rule: Domain Model & Currency Logic
- **Rich Domain Model:** Business rules and state mutations must be encapsulated within Domain Entities (e.g., `Card.RecordTransaction()`). 
- **Specification Pattern:** Encapsulate complex business rules using composable Specifications (`And`, `Or`, `Not`). Expose `Expression<Func<T, bool>>` to allow EF Core to translate rules into SQL.
- **Data Types:** Always use `decimal` for financial amounts and `Guid` for identifiers (`CardId`, `TransactionId`). Never use `float` or `double`.
- **Transaction Conversion:** Must find an exchange rate dated on or before the transaction date, strictly within a 6-month window. Return an error if no rate is found.
- **Available Balance:** Calculated as credit limit minus total transactions. Always use the **latest** available exchange rate for the target currency.