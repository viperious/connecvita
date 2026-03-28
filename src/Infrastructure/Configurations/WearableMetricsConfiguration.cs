using Connecvita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connecvita.Infrastructure.Configurations;

public class WearableMetricsConfiguration : IEntityTypeConfiguration<WearableMetrics>
{
    public void Configure(EntityTypeBuilder<WearableMetrics> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Platform)
            .IsRequired();

        builder.Property(x => x.Tags)
    .HasConversion(
        v => string.Join(',', v),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()
        ));

        builder.Property(x => x.SessionData)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.WorkoutSummary)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.UserProfileId, x.RecordedAt });
    }
}