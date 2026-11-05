using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;

namespace Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyBillingConfiguration(this ModelBuilder builder)
    {
        builder.Entity<PaymentTransaction>().HasKey(t => t.Id);
        builder.Entity<PaymentTransaction>().Property(t => t.Id)
            .HasConversion(new StronglyTypedIdConverter<PaymentTransactionId, int>(id => id.Value,
                value => new PaymentTransactionId(value)))
            .IsRequired()
            .ValueGeneratedOnAdd();
        builder.Entity<PaymentTransaction>().Property(t => t.UserId)
            .HasConversion(new StronglyTypedIdConverter<UserId, int>(id => id.Value, value => new UserId(value)))
            .IsRequired();
        builder.Entity<PaymentTransaction>().HasIndex(t => t.UserId);
        builder.Entity<PaymentTransaction>().Property(t => t.Plan)
            .HasConversion(plan => plan.Code, value => new SubscriptionPlan(value))
            .IsRequired()
            .HasMaxLength(20);
        builder.Entity<PaymentTransaction>().OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(10, 2).IsRequired();
            amount.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });
        builder.Entity<PaymentTransaction>().Navigation(t => t.Amount).IsRequired();
        builder.Entity<PaymentTransaction>().OwnsOne(t => t.Method, method =>
        {
            method.Property(m => m.Brand).HasColumnName("CardBrand").HasMaxLength(20).IsRequired();
            method.Property(m => m.Last4).HasColumnName("CardLast4").HasMaxLength(4).IsRequired();
            method.Property(m => m.HolderName).HasColumnName("CardHolderName").HasMaxLength(120).IsRequired();
            method.Property(m => m.Expiry).HasColumnName("CardExpiry").HasMaxLength(5).IsRequired();
        });
        builder.Entity<PaymentTransaction>().Navigation(t => t.Method).IsRequired();
        builder.Entity<PaymentTransaction>().Property(t => t.Status).HasConversion<string>().IsRequired()
            .HasMaxLength(20);
        builder.Entity<PaymentTransaction>().Property(t => t.AuthorizationCode).HasMaxLength(40);

        builder.Entity<Receipt>().HasKey(r => r.Id);
        builder.Entity<Receipt>().Property(r => r.Id)
            .HasConversion(new StronglyTypedIdConverter<ReceiptId, int>(id => id.Value, value => new ReceiptId(value)))
            .IsRequired()
            .ValueGeneratedOnAdd();
        builder.Entity<Receipt>().Property(r => r.TransactionId)
            .HasConversion(new StronglyTypedIdConverter<PaymentTransactionId, int>(id => id.Value,
                value => new PaymentTransactionId(value)))
            .IsRequired();
        builder.Entity<Receipt>().HasIndex(r => r.TransactionId).IsUnique();
        builder.Entity<Receipt>().HasOne<PaymentTransaction>().WithMany().HasForeignKey(r => r.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Receipt>().Property(r => r.UserId)
            .HasConversion(new StronglyTypedIdConverter<UserId, int>(id => id.Value, value => new UserId(value)))
            .IsRequired();
        builder.Entity<Receipt>().HasIndex(r => r.UserId);
        builder.Entity<Receipt>().Property(r => r.Type)
            .HasConversion(type => type.Value, value => new ReceiptType(value))
            .IsRequired()
            .HasMaxLength(10);
        builder.Entity<Receipt>().Property(r => r.Number).IsRequired();
        builder.Entity<Receipt>().HasIndex(r => new { r.Type, r.Number }).IsUnique();
        builder.Entity<Receipt>().Property(r => r.CustomerName).IsRequired().HasMaxLength(120);
        builder.Entity<Receipt>().Property(r => r.CustomerDocument).IsRequired().HasMaxLength(11);
        builder.Entity<Receipt>().OwnsOne(r => r.Total, total =>
        {
            total.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(10, 2).IsRequired();
            total.Property(m => m.Currency).HasColumnName("TotalCurrency").HasMaxLength(3).IsRequired();
        });
        builder.Entity<Receipt>().Navigation(r => r.Total).IsRequired();
        builder.Entity<Receipt>().Property(r => r.IssuedAt).IsRequired();
    }
}
