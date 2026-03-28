using Connecvita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connecvita.Infrastructure.Configurations;

public class WearableTokenConfiguration : IEntityTypeConfiguration<WearableToken>
{
    public void Configure(EntityTypeBuilder<WearableToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AccessToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.RefreshToken)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.UserId, x.Platform }).IsUnique();
    }
}