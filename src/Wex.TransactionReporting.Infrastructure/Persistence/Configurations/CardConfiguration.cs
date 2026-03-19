using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wex.TransactionReporting.Domain.Entities;

namespace Wex.TransactionReporting.Infrastructure.Persistence.Configurations;

public sealed class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreditLimit)
            .HasColumnType("numeric(18,4)")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();
    }
}
