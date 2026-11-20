# `how-long-ago-identity` (`HowIdentity`) — Context & Rules

## Overview
Identity Provider built with Duende IdentityServer 7, ASP.NET Core Identity, EF Core (`ApplicationDbContext`, `ConfigurationDbContext`, `PersistedGrantDbContext`), Dapper (`DapperContext`), Razor Pages, Quartz.NET jobs, gRPC client (`UserAccountGrpcClient`), and MassTransit RabbitMQ.

## Rules
- Always parameterize Dapper SQL queries (never interpolate emails, user IDs, or role IDs into SQL strings).
- `DapperContext` must reuse a single `NpgsqlDataSource` instance and dispose it properly.
- Wrap gRPC calls in `UserAccountGrpcClient` with `try/catch (RpcException)` and return `GrpcResult.Error` so `UserAccountService` can fall back to RabbitMQ (`UserAccountProducer`) without crashing user registration or admin operations.
- When gRPC registration succeeds, update `user_microservices.confirm_existing = true`.
- Prevent privilege escalation in `SuperAdminUserService.UpdateUserRoles` by filtering out `SuperAdmin` from roles that can be added or removed via UI.
