namespace HowIdentity.Data.EntityConfiguration;

using Entity;
using HowCommon.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HowUserConfiguration : IEntityTypeConfiguration<HowUser>
{
    public void Configure(EntityTypeBuilder<HowUser> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IsDeleted);
        builder.HasIndex(u => u.IsSuspended);
        
        builder.Property(u => u.ExistInServices).HasConversion<int>();
        builder.Property(u => u.ExistInServices).IsRequired();
        builder.Property(u => u.ExistInServices).HasDefaultValue(new [] { MicroServicesEnum.IdentityServer });
    }
}