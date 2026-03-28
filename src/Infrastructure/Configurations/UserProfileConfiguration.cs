using Connecvita.Domain.Entities;
using Connecvita.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connecvita.Infrastructure.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Bio)
            .HasMaxLength(2000);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        // Fix decimal precision warnings
        builder.OwnsOne(x => x.PhysicalAttributes, pa =>
        {
            pa.Property(p => p.HeightCm).HasColumnName("HeightCm").HasPrecision(5, 2);
            pa.Property(p => p.WeightKg).HasColumnName("WeightKg").HasPrecision(5, 2);
            pa.Property(p => p.BloodType).HasColumnName("BloodType");
            pa.Property(p => p.GeneticData).HasColumnName("GeneticData");
        });



        builder.OwnsOne(x => x.LatestHealthScores, hs =>
        {
            hs.Property(h => h.SleepScore).HasColumnName("SleepScore");
            hs.Property(h => h.ReadinessScore).HasColumnName("ReadinessScore");
            hs.Property(h => h.ActivityScore).HasColumnName("ActivityScore");
            hs.Property(h => h.RestingHeartRate).HasColumnName("RestingHeartRate");
            hs.Property(h => h.HRV).HasColumnName("HRV");
            hs.Property(h => h.BodyTemperature).HasColumnName("BodyTemperature");
            hs.Property(h => h.LastUpdated).HasColumnName("HealthScoresLastUpdated");
        });

        // Fix collection value comparer warnings
        builder.Property(x => x.PhotoUrls)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                ));

        builder.Property(x => x.ConnectedPlatforms)
            .HasConversion(
                v => string.Join(',', v.Select(p => p.ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(Enum.Parse<Domain.Enums.WearablePlatform>)
                      .ToList(),
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<Domain.Enums.WearablePlatform>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                ));

        builder.HasMany(x => x.WearableHistory)
            .WithOne()
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}