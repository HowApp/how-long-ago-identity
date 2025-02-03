
To add initial migration for Duende Identity Server

```csharp
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
```

To add initial migration for Application User
```csharp
dotnet ef migrations add InitialIdentityServerDbMigration -c ApplicationDbContext -o Data/Migrations

// to remove last migration if not apply
dotnet ef migrations remove --context ApplicationDbContext
```

To seed test users run
```
dotnet run /seed
```

example of secrets to run app
```json
{
  "BaseApplicationSettings": {
    "AllowedOrigins": [
      "https://localhost:7015",
      "https://localhost:7560",
      "https://localhost:7060",
      "http://localhost:5024",
      "http://localhost:5560",
      "http://localhost:5060"
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost; Port=5432; Database=/your_database_name/; Username=/your_username/; Password=/your_password/;"
  },
  "AdminCredentials": {
    "Email": "bobr@example.com",
    "Name": "SuperAdmin",
    "Password": "/your_password/"
  },
  "IdentityServerConfiguration": {
    "Clients": [
      {
        "ClientId": "how-api-swagger-client",
        "Secret": "/your_secret_for_swagger/"
      }
    ],
    "ApiResources": [
      {
        "ClientId": "resource.how-api",
        "Secret": "/your_secret_for_resource/"
      }
    ]
  },
  "RabbitMqConfiguration": {
    "Host": "127.0.0.1",
    "User": "admin",
    "Password": "admin",
    "Port": 5672
  }
}
```