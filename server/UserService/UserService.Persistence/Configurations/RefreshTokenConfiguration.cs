using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Persistence.Configurations;


public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");
        
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.CreatedDate)
            .IsRequired()
            .HasConversion(
                v => v.ToUniversalTime(), // Convert to UTC before save
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // Convert to UTC after upload

        builder.Property(r => r.ExpiryDate)
            .IsRequired()
            .HasConversion(
                v => v.ToUniversalTime(),  // Convert to UTC before save
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // // Convert to UTC after upload
        
        builder.Property(r => r.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.HasOne(r => r.User)
            .WithOne(u => u.RefreshToken)
            .HasForeignKey<RefreshToken>(r => r.UserId);
    }
}