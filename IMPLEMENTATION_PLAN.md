# How Long Ago (`how_app`) — Master Implementation Plan

This document outlines the phased implementation plan to fix all critical bugs, security flaws, and resource leaks identified in `PROBLEMS.md`, and to implement the main features defined in `MAIN_FEATURES.md`.

---

## Phase 1: `how-long-ago-common` & Local Package Feed Setup
1. Fix reversed arguments in `CertificateManagerBase.cs` (`ArgumentException.ThrowIfNullOrEmpty(CertPassword, nameof(CertPassword))` etc.).
2. Clean up empty or redundant files if any.
3. Build and pack `HowCommon` (version `1.0.8`) into `/home/mykhailo/dev/pet/how_app/.local-nuget` and configure `nuget.config` in `how-long-ago` and `how-long-ago-identity` so all projects restore and build deterministically without depending on external Azure DevOps PAT expiry.

---

## Phase 2: `how-artifacts` Secret Sanitization
1. Replace the plaintext Azure DevOps PAT in `how-artifacts/azure_devops.txt` with a redacted placeholder `<YOUR_AZURE_DEVOPS_PAT>`.
2. Sanitize sensitive passwords in `how-artifacts/how-identity.json` and `how-artifacts/how-api.json` with safe local development placeholders and clear documentation in `how-artifacts/README.md`.

---

## Phase 3: `how-long-ago-identity` (`HowIdentity`) Bug Fixes & Features
1. **Fix `DapperContext` Connection Pool Leak**:
   - Store a single `NpgsqlDataSource` instance on `DapperContext` (or register `DapperContext` with `IDisposable` disposing `_dataSource`) so it doesn't build a new connection pool on every query.
2. **Fix Registration Page & Validation**:
   - Fix `Pages/Account/Register/Index.cshtml` form target from `asp-page="/Account/Create/Index"` to `asp-page="/Account/Register/Index"`.
   - Add `[Required]` and `[EmailAddress]` to `InputModel.Email` in `Pages/Account/Register/InputModel.cs`.
   - Fix duplicate email error key (`"Input.Email"`) and wrap `_userAccount.SendRegisterUserRequest(user.Id)` in a resilient flow.
3. **Fix gRPC Client Resilience & Microservice Sync**:
   - Wrap gRPC calls in `UserAccountGrpcClient.cs` (`SendRegisterUserRequest`, `SendDeleteUserRequest`, `SendSuspendUserRequest`) in `try/catch (Exception ex)` returning `GrpcResult.Error` so `UserAccountService` falls back cleanly to RabbitMQ (`_userAccountProducer`) when `how-api` is offline.
   - Update `user_microservices.confirm_existing = true` when gRPC user registration succeeds (`GrpcResult.Success`).
   - Rethrow exceptions in `UserRegiserBulkResponseConsumer.cs` so MassTransit retry policy works.
4. **Fix SQL Injection & Privilege Escalation in `SuperAdminUserService` & `UpdateUserMicroservicesJob`**:
   - Parameterize `@AdminEmail` in `SuperAdminUserService.GetUsers` and `GetUserById`.
   - Parameterize role insertions in `SuperAdminUserService.UpdateUserRoles` and filter out `IdentityRoleConstant.Role.SuperAdmin.Id` from `roleToAdd`.
   - Parameterize bulk inserts in `UpdateUserMicroservicesJob.cs`.
5. **Implement SuperAdmin User Search & Pagination + Fix `EditUser` Page**:
   - Add email filtering (`@EmailFilter`) and pagination (`page`, `size`) to `SuperAdminUserService.GetUsers`.
   - Fix `Pages/SuperAdmin/AppUsers/Index.cshtml` search form and pagination controls.
   - Add `[TempData]` to `ErrorsString` in `Pages/SuperAdmin/EditUser/Index.cshtml.cs`, pass `new { id = UserId }` on `RedirectToPage`, and guard against null `AppUser` in `EditUser/Index.cshtml`.
6. **Verify Build**:
   - Run `dotnet build` on `HowIdentity.sln` and verify 0 errors.

---

## Phase 4: `how-long-ago` (Main API: `How.Common`, `How.Core`, `How.Server`) Bug Fixes & Features
1. **Fix `DapperConnection` Connection Pool Leak & EF Core Registration**:
   - Store lazy/singleton `NpgsqlDataSource` instances for `_mainDataSource` and `_tempDataSource` in `DapperConnection.cs` (implementing `IDisposable`).
   - Remove conflicting `AddDbContextFactory` registrations in `ServiceCollectionExtensions.cs`.
2. **Fix Dapper SQL & Access Builders**:
   - Fix `@created_by_id` -> `@createdById` in `EventAccessQueryAccessBuilder.cs` and `RecordAccessAccessBuilder.cs`.
   - Fix `GetSharedUsersQueryHandler.cs` correlated subquery: `WHERE su.user_owner_id = @userId AND su.user_shared_id = u.user_id`.
   - Fix `CheckExistQueryHandler.cs` to use the correct primary key column (`user_id` for `users`, `id` for `events`/`records`) with safe whitelist validation.
   - Fix `GetRecordsPaginationQueryHandler.cs` and `GetRecordsPaginationPublicQueryHandler.cs`:
     - Fix Dapper `splitOn` parameter.
     - Guard against null `image.MainHash` when a record has no images.
     - Add `ORDER BY r.id DESC, ri.position ASC`.
     - Add `ImageId` (`ri.id`) to `ImageModelDTO` so clients can reference record images.
   - Fix `GetUserInfoByUserNameQueryHandler.cs` to exclude deleted/suspended users (`u.is_deleted = FALSE AND u.is_suspended = FALSE`).
3. **Fix Pagination, Validators, & Record Like Access**:
   - Fix `PaginationDTO.Page` setter (`_page = value < 1 ? 1 : value;`).
   - Add `MaximumLength(1024)` to `CreateEventRequestDTOValidator.cs`, fix `LessThanOrEqualTo(PaginationDTO.MaxSize)` and `.GreaterThan(0)` in pagination validators, fix `UpdateRecordImagesRequestDTOValidator.cs` null safety, and fix `UpdateUserInfoRequestDTOValidator.cs` message.
   - Fix `RecordService.UpdateLikeState` to check `FilterByAccessType(EventAccessType.Public)` so authenticated users can like/dislike records on public active events.
4. **Fix Storage, Image Processing, SignalR, & Controllers**:
   - Add missing `return;` in `BackgroundImageProcessing.EventImageProcessing` when `temporaryImage.Data is null` and pass file IDs via local variables instead of mutable instance fields.
   - Remove `GC.Collect()` calls in `ImageStorageService.cs`, use `StoragePathHelper.Images.Thumbnail` for thumbnails, and append a unique Guid/random suffix to prevent `ix_storage_files_path` collisions during batch uploads.
   - Fix `StorageController.GetByte` and `GetStream` to check `if (result.Failed) return HttpResult(result);`.
   - Fix `BaseController.HttpResult` to use `result.Code > 0 ? result.Code : (int)HttpStatusCode.BadRequest`.
   - Fix `IdentityService.GetCurrentUserInfo()` to handle multiple claims of the same type (`GroupBy` + `string.Join(", ", ...)`).
   - Fix `FileProcessingHub.cs` to check `Context.User.FindFirstValue("sub") ?? Context.User.FindFirstValue(ClaimTypes.NameIdentifier)` and configure `JwtBearerEvents.OnMessageReceived` in `ServiceCollectionExtensions.cs` to read `access_token` for `/hubs/fileProcessing`.
   - Fix `QueueHostedService.cs` to dequeue and process tasks without waiting for 5 items to accumulate before cleaning up completed tasks.
   - Remove empty 0-byte `Core/Infrastructure/CertificateManagement/CertificateManagerBase.cs`.
5. **Implement Feature 1 (Saved Events Listing)**:
   - Add `Saved` to `ApiRequestAccessFilter` (`Core/Enums/ApiRequestAccessFilter.cs`) and `IncludeSaved` to `InternalAccessFilter` (`Core/Enums/InternalAccessFilter.cs`).
   - Update `GetEventsPaginationQueryHandler.cs` to filter by `EXISTS (SELECT 1 FROM saved_events se WHERE se.event_id = e.id AND se.saved_by_user_id = @created_by_id)` when `Access == InternalAccessFilter.IncludeSaved` (and `e.is_deleted = FALSE`).
   - Update `EventService.GetEventsPagination` to map `ApiRequestAccessFilter.Saved` -> `InternalAccessFilter.IncludeSaved`.
6. **Verify Build**:
   - Run `dotnet build` on `How.sln` and verify 0 errors.

---

## Phase 5: `how-long-ago-client` (`HowClient`) Bug Fixes & Features
1. **Fix Core Bugs & Memory Leaks**:
   - Fix `PaginationDTO.Page` setter (`_page = value < 1 ? 1 : value;`).
   - Fix modal parameter casing (`"OnClickCallback"`) in `Dashboard.razor` and `DashboardEvent.razor`, and `await OnClickCallback.InvokeAsync(...)` in `CreateUpdateEventModal.razor`.
   - Implement `IDisposable` on `Home.razor`, `Event.razor`, `HomePrivate.razor`, `EventPrivate.razor`, `Dashboard.razor`, and `DashboardEvent.razor` to stop/dispose `_timer` and unsubscribe `NotificationService.OnError -= HandleError`.
   - Use `await InvokeAsync(StateHasChanged)` in `OnTimerElapsed` and guard against null `_event`.
   - Add `@implements IAsyncDisposable` to `HubTest.razor` and use `AppConfigurations.BackendUrl`.
   - Fix unauthenticated redirect in `HomePrivate.razor` (`Rout.Home.HomePublicPage`).
   - Fix zero-records pagination bug (`_currentPageNumber = _totalPage == 0 ? 1 : _totalPage;`) in `Event.razor`, `EventPrivate.razor`, and `DashboardEvent.razor`.
   - Fix `EventPrivateService.UploadImage` to return `false` when `response.Failed` is `true` and wrap in `try/catch`.
   - Fix Dislike icon in `HomePrivate.razor` and `EventPrivate.razor` (`IconName.HandThumbsDownFill` / `IconName.HeartCrack`).
2. **Implement Feature 1 (Saved Events View in Dashboard)**:
   - Add `Saved = 3` to `ApiRequestAccessFilter.cs` and map `ApiRequestAccessFilter.Saved => "saved"` in `ApiAccessHelper.cs`.
   - Add `Saved` option to `Dashboard.razor` access dropdown, auto-reload events when filter selections change, and only show owner-only Event delete/status/access switches when `AccessFilter == ApiRequestAccessFilter.Own`.
3. **Implement Feature 2 (Full Record Management & Record Likes in Blazor Client)**:
   - Add `CreateRecord`, `UpdateRecord`, `DeleteRecord`, `UpdateRecordLikeState`, and `UploadRecordImages` to `IRecordPrivateService` and `RecordPrivateService` (with request DTOs `CreateUpdateRecordRequestDTO`, `UploadRecordImagesRequestDTO`).
   - Create `Components/Modals/CreateUpdateRecordModal.razor`.
   - Update `DashboardEvent.razor` with buttons and modals to **Create Record**, **Edit Record**, **Delete Record**, and **Upload Record Images**.
   - Update `EventPrivate.razor` with **Like / Dislike Record** buttons wired to `RecordPrivateService.UpdateRecordLikeState`.
4. **Implement Feature 3 (User Profile & Shared Access Management Page)**:
   - Create `Services/Private/Account/IAccountPrivateService.cs`, `AccountPrivateService.cs`, `Services/Private/SharedUser/ISharedUserPrivateService.cs`, and `SharedUserPrivateService.cs` (and corresponding DTOs).
   - Register the new services in `Extensions/ServiceCollectionExtensions.cs`.
   - Create `Pages/Private/Profile.razor` (`/profile`) allowing users to update their First Name, Last Name, Avatar Image, and manage Shared Users (search by name/nickname, add shared user, delete shared user).
   - Add `/profile` navigation item to `Layout/MainLayout.razor`.
5. **Verify Build**:
   - Run `dotnet build` on `HowClient.sln` and verify 0 errors.

---

## Phase 6: Final Verification, Git Commit & Push
1. Build all 4 solutions (`HowCommon.csproj`, `How.sln`, `HowIdentity.sln`, `HowClient.sln`) to confirm clean compilation across the entire workspace.
2. Commit and push changes across the repositories.
