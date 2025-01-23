
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

To add common nuget need add source to nuget.config in project folder
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="how-long-ago-common" value="https://pkgs.dev.azure.com/MykhailoRospopchuk/HowApp/_packaging/how-long-ago-common/nuget/v3/index.json" />
    </packageSources>
</configuration>
```