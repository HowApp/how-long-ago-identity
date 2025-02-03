namespace HowIdentity.Data.EntityConfiguration;

using Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserMicroservicesConfiguration : IEntityTypeConfiguration<UserMicroservices>
{
    public void Configure(EntityTypeBuilder<UserMicroservices> builder)
    {
        builder.HasKey(x => new { x.UserId, x.MicroService });

        builder.HasIndex(x => x.ConfirmExisting);
    }
}