using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wex.TransactionReporting.Domain.Entities;

namespace Wex.TransactionReporting.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.AmountUsd)
            .HasColumnType("numeric(18,4)")
            .IsRequired();

        builder.Property(t => t.IdempotencyKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique();

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasOne<Card>()
            .WithMany()
            .HasForeignKey(t => t.CardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
