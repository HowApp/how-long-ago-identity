# How Long Ago (`how_app`) — Comprehensive Problems & Audit Report

This document catalogs the critical bugs, runtime crashes, SQL defects, security vulnerabilities, performance bottlenecks, and code quality issues discovered across all five repositories in the `how_app` workspace.

---

## 1. Critical Runtime Bugs & Broken SQL Queries (`how-long-ago` Main API)

1. **Dapper Parameter Name Mismatch (`@createdById` vs `@created_by_id`) Breaks `IncludeCreatedBy` Queries**
   - **Location**: `how-long-ago/Core/Infrastructure/Builders/EventAccessQueryAccessBuilder.cs` (L41, L48) & `RecordAccessAccessBuilder.cs` (L46, L53)
   - **Problem**: Both builders add `_parameters.Add("@createdById", userId);` and use `@createdById` in the `IncludeShared` branch, but write `e.created_by_id = @created_by_id` in the `IncludeCreatedBy` branch. Any owner-only check throws `PostgresException: 42P02: Parameter "@created_by_id" must be defined`.

2. **Uncorrelated `EXISTS` Subquery in `GetSharedUsersQueryHandler` Leaks Entire `users` Table**
   - **Location**: `how-long-ago/Core/CQRS/Queries/SharedUser/GetSharedUsers/GetSharedUsersQueryHandler.cs` (L41–45)
   - **Problem**: `WHERE EXISTS (SELECT 1 FROM shared_users su WHERE su.user_owner_id = @userId)` is missing `AND su.user_shared_id = u.user_id`. As soon as a user shares with 1 person, this query returns **every user in the database**.

3. **`CheckExistQueryHandler` Queries Non-Existent Column `id` on `users` Table (`CreateSharedUser` 100% Broken)**
   - **Location**: `how-long-ago/Core/CQRS/Queries/General/CheckExist/CheckExistQueryHandler.cs` (L26–37) & `SharedUserService.cs` (L39–43)
   - **Problem**: `users` table has primary key `user_id`, not `id`. Executing `SELECT EXISTS(SELECT 1 FROM users WHERE id = @id)` always throws PostgreSQL error `42703: column "id" does not exist`. Also interpolates `{request.Table}` directly into SQL.

4. **Dapper Multi-Mapping `splitOn` Crash & Missing Image Metadata in Record Pagination**
   - **Location**: `how-long-ago/Core/CQRS/Queries/Record/GetRecordsPagination/GetRecordsPaginationQueryHandler.cs` (L33–41, L89–115) & `GetRecordsPaginationPublicQueryHandler.cs` (L35–42, L102–129)
   - **Problem**:
     - Passes `splitOn: $"id,{nameof(ImageModelDTO.MainHash)}"` for a 2-type mapping (`QueryAsync<RecordItem...DTO, ImageModelDTO, ...>`). Dapper looks for a second `"id"` column and throws `ArgumentException`. It must be `splitOn: nameof(ImageModelDTO.MainHash)` (or `ImageId` when included).
     - When a record has no images, `LEFT JOIN` yields an `ImageModelDTO` with `null` hashes which gets added to `currentRecord.Images` unless `image.MainHash is not null` is checked.
     - Does not `ORDER BY ri.position ASC`, losing image ordering.

5. **`PaginationDTO.Page` Setter Overwrites Clamped Value**
   - **Location**: `how-long-ago/Common/DTO/PaginationDTO.cs` (L12–20) & `how-long-ago-client/Common/DTO/PaginationDTO.cs` (L12–20)
   - **Problem**: `if (value < 1) { _page = 1; } _page = value;` overwrites `_page = 1` with the negative/zero `value`, causing negative SQL `OFFSET` crashes (`2201X: OFFSET must not be negative`).

6. **Missing `return;` Causes `NullReferenceException` in `BackgroundImageProcessing.EventImageProcessing`**
   - **Location**: `how-long-ago/Core/Services/BackgroundImageProcessing/BackgroundImageProcessing.cs` (L183–191)
   - **Problem**: When `temporaryImage.Data is null`, it notifies the user `"Temporary Image is null!"` but forgets to `return;`, immediately crashing on `temporaryImage.Data.Content`.

7. **`NullReferenceException` in `StorageController.GetByte` & `GetStream` When File Not Found**
   - **Location**: `how-long-ago/Server/Controllers/Public/StorageController.cs` (L21–36)
   - **Problem**: Does not check `if (result.Failed) return HttpResult(result);` before accessing `result.Data.Content`, throwing a 500 `NullReferenceException` instead of returning 404.

8. **SignalR `FileProcessingHub` Claim Mismatch & Missing WebSocket Token Extraction**
   - **Location**: `how-long-ago/Core/Infrastructure/Hubs/FileProcessingHub.cs` (L20) & `how-long-ago/Server/Extensions/ServiceCollectionExtensions.cs` (L157)
   - **Problem**: `MapInboundClaims = false` keeps the JWT subject claim as `"sub"`, while `FileProcessingHub` looks up `ClaimTypes.NameIdentifier` (which is always `null`), placing all users into the `"unauthorized"` group so `NotifyUser(userId, ...)` never reaches anyone. Also, `JwtBearerEvents.OnMessageReceived` is not configured to read `access_token` from query string for SignalR `/hubs/fileProcessing`.

9. **`IdentityService.GetCurrentUserInfo()` Crashes on Duplicate Claim Types**
   - **Location**: `how-long-ago/Core/Services/Identity/IdentityService.cs` (L38)
   - **Problem**: `user.Claims.ToDictionary(c => c.Type, c => c.Value)` throws `ArgumentException: An item with the same key has already been added` when a token has multiple `scope`, `role`, or `amr` claims.

10. **Unique Index Collision in `ImageStorageService` & `FileStorageService`**
    - **Location**: `how-long-ago/Core/Services/Storage/ImageStorage/ImageStorageService.cs` (L58–61, L82, L194–195, L216) & `FileStorageService.cs` (L59)
    - **Problem**: Uses `StoragePathHelper.Images.Image` instead of `StoragePathHelper.Images.Thumbnail` for thumbnails, and batch uploads with identical filenames in the same clock tick collide on `ix_storage_files_path` / `ix_storage_files_hash`.

11. **`BaseController.HttpResult` Ignores `result.Code`**
    - **Location**: `how-long-ago/Server/Controllers/BaseController.cs` (L10–28)
    - **Problem**: Always returns HTTP `400 BadRequest` on failure even when services set `result.Code = 404` or `418`.

12. **`RecordService.UpdateLikeState` Blocks Regular Users from Liking Public Records**
    - **Location**: `how-long-ago/Core/Services/Record/RecordService.cs` (L217–219)
    - **Problem**: Uses `FilterCreatedBy(_userService.UserId, InternalAccessFilter.IncludeShared)` instead of `FilterByAccessType(EventAccessType.Public)`, preventing authenticated users from liking records on public events!

---

## 2. Critical Bugs in `how-long-ago-client` (Blazor WebAssembly)

1. **Modal Parameter Case Mismatch (`OnclickCallback` vs `OnClickCallback`) Breaks Event Create/Update & Image Upload**
   - **Location**: `how-long-ago-client/Pages/Private/Dashboard.razor` (L249) & `DashboardEvent.razor` (L267, L275) vs `CreateUpdateEventModal.razor` (L41) & `UploadImageModal.razor` (L8)
   - **Problem**: Modals declare `[Parameter] public EventCallback<...> OnClickCallback` (capital `C`), while callers pass `"OnclickCallback"` (lowercase `c`), breaking modal submission callbacks.

2. **Severe Memory Leaks Across All 6 Pages (Missing `IDisposable` for Timers & Singleton Event Subscriptions)**
   - **Location**: `Home.razor`, `Event.razor`, `HomePrivate.razor`, `EventPrivate.razor`, `Dashboard.razor`, `DashboardEvent.razor`, `HubTest.razor`
   - **Problem**:
     - None of the 6 timer pages implement `IDisposable`. `System.Timers.Timer` instances tick every second forever in the background after navigation.
     - `HomePrivate`, `EventPrivate`, `Dashboard`, and `DashboardEvent` subscribe to singleton `InternalNotificationService.OnError += HandleError;` and never unsubscribe (`-= HandleError`), leaking page instances and multiplying error callbacks.
     - `OnTimerElapsed` calls `StateHasChanged()` directly from a background timer thread without `InvokeAsync(StateHasChanged)` and can dereference `_event` before it finishes loading.
     - `HubTest.razor` defines `DisposeAsync()` without `@implements IAsyncDisposable`.

3. **Infinite Self-Redirect Loop in `HomePrivate.razor`**
   - **Location**: `how-long-ago-client/Pages/Private/HomePrivate.razor` (L138–142)
   - **Problem**: When unauthenticated, `HomePrivate.razor` (`/authorized`) redirects to `Rout.Home.HomePrivatePage` (`/authorized`) instead of `Rout.Home.HomePublicPage` (`/public`).

4. **Zero-Records Pagination Bug (`_currentPageNumber = 0`)**
   - **Location**: `Event.razor` (L160–203), `EventPrivate.razor` (L188–230), `DashboardEvent.razor` (L204–246)
   - **Problem**: When an event has 0 records (`_totalPage == 0`), `_currentPageNumber` is set to `_totalPage` (`0`) instead of `_totalPage == 0 ? 1 : _totalPage`.

5. **`EventPrivateService.UploadImage` Always Returns `true` on Failure**
   - **Location**: `how-long-ago-client/Services/Private/Event/EventPrivateService.cs` (L149–157)
   - **Problem**: Returns `true` even when `response.Failed` is `true`, showing a false `"Image start processing"` success toast.

6. **Dislike Button Uses Same Heart Icon as Like Button**
   - **Location**: `HomePrivate.razor` (L67) & `EventPrivate.razor` (L43)
   - **Problem**: Both Like and Dislike render `IconName.Heart` with `BadgeColor.Danger`.

---

## 3. Critical Bugs & Security Flaws in `how-long-ago-identity` & `how-long-ago-common`

1. **Broken Form Post Target on Registration Page**
   - **Location**: `how-long-ago-identity/HowIdentity/Pages/Account/Register/Index.cshtml` (L14)
   - **Problem**: `<form asp-page="/Account/Create/Index">` posts to a non-existent `/Account/Create/Index` page instead of `/Account/Register/Index`.

2. **Unhandled gRPC `RpcException` Breaks Fallback to RabbitMQ & Crashes Registration**
   - **Location**: `how-long-ago-identity/HowIdentity/Infrastructure/GrpcCommunication/Client/UserAccountGrpcClient.cs` (L27–78)
   - **Problem**: `UserAccountGrpcClient` does not wrap gRPC calls in `try/catch`. When `how-api` (`https://localhost:7061`) is down, `RpcException` crashes the caller instead of returning `GrpcResult.Error` to trigger the MassTransit RabbitMQ fallback in `UserAccountService`.
   - Furthermore, when gRPC succeeds, `user_microservices.confirm_existing` is never updated to `true`.

3. **SQL Injection Vulnerabilities via String Interpolation in Dapper**
   - **Location**: `how-long-ago-identity/HowIdentity/Services/SuperAdmin/SuperAdminUserService.cs` (L65, L103, L228–237) & `UpdateUserMicroservicesJob.cs` (L115–119)
   - **Problem**: Interpolates `_adminCredentials.Value.Email`, `userId`, and role IDs directly into raw SQL strings instead of using Dapper parameters.

4. **Privilege Escalation in `SuperAdminUserService.UpdateUserRoles`**
   - **Location**: `how-long-ago-identity/HowIdentity/Services/SuperAdmin/SuperAdminUserService.cs` (L211–212)
   - **Problem**: Filters out `SuperAdmin` from `roleToDelete`, but does **not** filter `SuperAdmin` out of `roleToAdd`!

5. **Broken SuperAdmin Pages (`AppUsers` & `EditUser`)**
   - **Location**: `HowIdentity/Pages/SuperAdmin/AppUsers/Index.cshtml` (L29–32), `AppUsers/Index.cshtml.cs` (L23), `EditUser/Index.cshtml.cs` (L24, L40–59, L74)
   - **Problem**:
     - `AppUsers` search button links to `/ServerSideSessions/Index` and ignores `EmailFilter`.
     - `EditUser` is missing `[TempData]` on `ErrorsString`, crashes with `NullReferenceException` when user is not found, and calls `RedirectToPage()` without `new { id }`.

6. **Reversed Arguments in `ArgumentException.ThrowIfNullOrEmpty` in `how-long-ago-common`**
   - **Location**: `how-long-ago-common/Infrastructure/CertificateManagement/CertificateManagerBase.cs` (L98–104)
   - **Problem**: Passes `ArgumentException.ThrowIfNullOrEmpty(nameof(CertPassword), CertPassword)` — putting the constant string `"CertPassword"` as the value to check, so it never throws when `CertPassword` is null or empty.

---

## 4. Severe Performance & Resource Leaks Across Microservices

1. **`NpgsqlDataSource` Connection Pool Leak on Every Query**
   - **Location**: `how-long-ago/Core/Database/DapperConnection.cs` (L16–33) & `how-long-ago-identity/HowIdentity/Data/DapperContext.cs` (L14–22)
   - **Problem**: Both classes instantiate a new `NpgsqlDataSourceBuilder(_connectionString).Build()` on **every single query** without disposing it. In Npgsql 7/8, each `NpgsqlDataSource` creates its own physical connection pool, leaking sockets and database connections until PostgreSQL exhausts `max_connections`.

2. **Forced Full Garbage Collection (`GC.Collect()`) on Every Image Upload**
   - **Location**: `how-long-ago/Core/Services/Storage/ImageStorage/ImageStorageService.cs` (L103, L140, L176)
   - **Problem**: Calls `GC.Collect()` inside `finally` blocks on every image upload, causing Stop-The-World Gen 2 GC pauses.

3. **Conflicting `AddDbContext` + `AddDbContextFactory` Registration**
   - **Location**: `how-long-ago/Server/Extensions/ServiceCollectionExtensions.cs` (L180–213)
   - **Problem**: Registers `AddDbContextFactory<BaseDbContext>` after `AddDbContext<BaseDbContext>` without `.UseNodaTime()` or `.UseSnakeCaseNamingConvention()`, overriding `DbContextOptions`.

4. **`QueueHostedService` Blocks Dequeue Loop Until `MaxDegreeParallelism` Items Arrive**
   - **Location**: `how-long-ago/Core/Infrastructure/Background/Workers/QueueHostedService.cs` (L26–63)
   - **Problem**: Inner `while (tasks.Count < MaxDegreeParallelism)` awaits `_backgroundTaskQueue.DequeueAsync` unconditionally, holding completed tasks in memory until 5 tasks have been queued.

---

## 5. Exposed Secrets in `how-artifacts`

- **Location**: `how-artifacts/azure_devops.txt`, `how-artifacts/how-identity.json`, `how-artifacts/how-api.json`
- **Problem**: Contains a plaintext Azure DevOps PAT and hardcoded passwords/secrets in git history. Must be sanitized with placeholder values.
