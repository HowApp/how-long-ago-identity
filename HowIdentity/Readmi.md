
To add initial migration for Duende Identity Server

```csharp
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
```

To add initial migration for Application User
```csharp
dotnet ef migrations add InitialIdentityServerDbMigration -c ApplicationDbContext -o Data/Migrations
```

To seed test users run
```
dotnet run /seed
```