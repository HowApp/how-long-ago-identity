namespace HowIdentity.Data.Extensions;

using System.Globalization;
using Entity;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;
using Seeds;

public static class ModelBuilderExtensions
{
    public static void SetOnDeleteRule(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.GetForeignKeys()
                .Where(fk => 
                    !fk.IsOwnership && 
                    fk.DeleteBehavior == DeleteBehavior.Cascade)
                .ToList()
                .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);
        }
    }
    
    public static ModelBuilder UseSnakeCaseNamingConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // table
            var tableName = entity.GetTableName() ?? string.Empty;
            var translatedTableName = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(tableName, CultureInfo.InvariantCulture);
            entity.SetTableName(translatedTableName);

            // column
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                var translatedName = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(columnName, CultureInfo.InvariantCulture);
                property.SetColumnName(translatedName);
            }

            // primary and alternate key
            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName() ?? string.Empty;
                var translatedName = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(keyName, CultureInfo.InvariantCulture);
                key.SetName(translatedName);
            }

            // foreign key
            foreach (var key in entity.GetForeignKeys())
            {
                var constraintName = key.GetConstraintName() ?? string.Empty;
                var translatedName = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(constraintName, CultureInfo.InvariantCulture);
                key.SetConstraintName(translatedName);
            }

            // index
            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName() ?? string.Empty;
                var translatedName = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(indexName, CultureInfo.InvariantCulture);
                index.SetDatabaseName(translatedName);
            }
        }

        return modelBuilder;
    }

    public static void SetIdentityName(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HowUser>(b =>
        {
            b.ToTable("Users");
        });

        modelBuilder.Entity<HowUserClaim>(b =>
        {
            b.ToTable("UserClaims");
        });

        modelBuilder.Entity<HowUserLogin>(b =>
        {
            b.ToTable("UserLogins");
        });

        modelBuilder.Entity<HowUserToken>(b =>
        {
            b.ToTable("UserTokens");
        });

        modelBuilder.Entity<HowRole>(b =>
        {
            b.ToTable("Roles");
        });

        modelBuilder.Entity<HowRoleClaim>(b =>
        {
            b.ToTable("RoleClaims");
        });

        modelBuilder.Entity<HowUserRole>(b =>
        {
            b.ToTable("UserRoles");
        });
    }

    public static void SetIdentityRule(this ModelBuilder modelBuilder)
    {
        modelBuilder.SeedRoles();
        
        modelBuilder.Entity<HowUser>(b =>
        {
            // Each User can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });
        
        modelBuilder.Entity<HowRole>(b =>
        {
            // Each Role can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });
        
        modelBuilder.Entity<HowUserRole>(b =>
        {
            // Primary key
            b.HasKey(r => new { r.UserId, r.RoleId });
        });
    }
}