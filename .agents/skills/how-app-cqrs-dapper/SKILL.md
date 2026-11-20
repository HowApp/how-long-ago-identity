---
name: how-app-cqrs-dapper
description: >-
  Guides authoring and debugging MediatR CQRS Commands, Queries, Access Builders,
  and Dapper PostgreSQL handlers in how-long-ago (How.Core and How.Server).
  Use when adding or fixing backend endpoints, SQL queries, or services.
---

# How App CQRS & Dapper Workflow

Use this skill when adding or modifying backend features in `how-long-ago`.

## Structure of a Feature in `how-long-ago`
1. **DTOs & Validators**:
   - Request/Response DTOs live in `Core/DTO/<Area>/`.
   - FluentValidation validators live alongside request DTOs and are auto-registered from `How.Core`.
2. **CQRS Commands & Queries**:
   - Commands implement `ICommand<Result>` or `ICommand<Result<T>>` in `Core/CQRS/Commands/<Area>/<CommandName>/`.
   - Queries implement `IQuery<Result<T>>` in `Core/CQRS/Queries/<Area>/<QueryName>/`.
   - Handlers inject `DapperConnection` and `ILogger<THandler>`.
3. **Dapper SQL Conventions**:
   - Use `await using var connection = await _dapper.InitConnectionAsync(cancellationToken);` (or `using var connection = _dapper.InitConnection();`).
   - All table and column names use `snake_case` (`events`, `records`, `record_images`, `storage_images`, `storage_files`, `liked_events`, `liked_records`, `saved_events`, `shared_users`, `users`).
   - Note that `users` has primary key `user_id` (not `id`).
   - Use `EventAccessQueryAccessBuilder` and `RecordAccessAccessBuilder` for ownership (`IncludeCreatedBy`), shared (`IncludeShared`), and saved (`IncludeSaved`) filtering.
4. **Services & Controllers**:
   - Services in `Core/Services/<Area>/` coordinate access checks, MediatR dispatch, and background queues.
   - Controllers in `Server/Controllers/<Group>/` inherit from `BaseController` and return `HttpResult(result)`.
