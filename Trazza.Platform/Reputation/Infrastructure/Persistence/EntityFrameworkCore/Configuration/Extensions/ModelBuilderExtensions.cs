using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyReputationConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Rating>().HasKey(r => r.Id);
        builder.Entity<Rating>().Property(r => r.Id)
            .HasConversion(new StronglyTypedIdConverter<RatingId, int>(id => id.Value, value => new RatingId(value)))
            .IsRequired()
            .ValueGeneratedOnAdd();
        builder.Entity<Rating>().Property(r => r.ShipmentId)
            .HasConversion(new StronglyTypedIdConverter<ShipmentId, int>(id => id.Value, value => new ShipmentId(value)))
            .IsRequired();
        builder.Entity<Rating>().Property(r => r.RaterId)
            .HasConversion(new StronglyTypedIdConverter<RaterId, int>(id => id.Value, value => new RaterId(value)))
            .IsRequired();
        builder.Entity<Rating>().HasIndex(r => new { r.ShipmentId, r.RaterId }).IsUnique();
        builder.Entity<Rating>().Property(r => r.RaterName).IsRequired().HasMaxLength(120);
        builder.Entity<Rating>().OwnsOne(r => r.Target, target =>
        {
            target.Property(t => t.UserId)
                .HasConversion(new StronglyTypedIdConverter<RatedUserId, int>(id => id.Value,
                    value => new RatedUserId(value)))
                .HasColumnName("TargetUserId")
                .IsRequired();
            target.Property(t => t.Role).HasColumnName("TargetRole").IsRequired().HasMaxLength(20);
            target.Property(t => t.Name).HasColumnName("TargetName").IsRequired().HasMaxLength(120);
        });
        builder.Entity<Rating>().Navigation(r => r.Target).IsRequired();
        builder.Entity<Rating>().Property(r => r.Score)
            .HasConversion(score => score.Value, value => new Score(value))
            .IsRequired();
        builder.Entity<Rating>().Property(r => r.Tags)
            .HasConversion(
                tags => string.Join(",", tags),
                value => value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (left, right) => left!.SequenceEqual(right!),
                    tags => tags.Aggregate(0, (hash, tag) => HashCode.Combine(hash, tag.GetHashCode())),
                    tags => tags.ToList()))
            .IsRequired()
            .HasMaxLength(200);
        builder.Entity<Rating>().Property(r => r.Comment).IsRequired().HasMaxLength(300);
    }
}
