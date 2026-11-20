# How Long Ago (`how_app`) — Main Features That Can Be Implemented

Based on an analysis of the existing domain model, unfinished `TODO` markers in the codebase, and the core purpose of a **"How Long Ago"** event & occurrence tracking platform, below are the main features that complete and elevate the application.

---

## Feature 1: Saved (Bookmarked) Events Listing & Dashboard Filtering (Backend + Frontend)
- **Why**:
  - `EventController.cs` (Line 202) explicitly has `// TODO add ability to get saved events`.
  - Users can already bookmark/unbookmark events (`POST /api/dashboard/event/{eventId}/saved/add` and `DELETE /api/dashboard/event/{eventId}/saved/delete`), and queries already compute `IsSavedByUser` and `SavedCount`.
  - However, there is no way for a user to view their Saved/Bookmarked events!
- **What to Implement**:
  - **Backend (`how-long-ago`)**:
    - Extend `InternalAccessFilter` (or `ApiRequestAccessFilter`) and `GetEventsPaginationQueryHandler` to support filtering by **`Saved`** events (events present in `saved_events` for the current `user_id`), or add dedicated support via `ApiRequestAccessFilter.Saved` (`GET /api/dashboard/event/list-pagination/saved`).
  - **Frontend (`how-long-ago-client`)**:
    - Extend `ApiRequestAccessFilter` enum (`Own`, `Shared`, `Saved`) and `ApiAccessHelper` (`"saved"`).
    - Update `Dashboard.razor` dropdown so users can switch between **Own**, **Shared**, and **Saved** events (and automatically reload events when filters change, hiding owner-only edit/delete controls on non-owned events).

---

## Feature 2: Full Record Management & Interactions in Blazor Client (`how-long-ago-client`)
- **Why**:
  - The backend `RecordController` (`how-long-ago/Server/Controllers/Dashboard/RecordController.cs`) already implements complete endpoints for:
    - `POST /api/dashboard/event/{eventId}/record/create` (Create Record)
    - `PUT /api/dashboard/event/{eventId}/record/{recordId}/update` (Update Record Description)
    - `PATCH /api/dashboard/event/{eventId}/record/{recordId}/like-state` (Like/Dislike Record)
    - `POST /api/dashboard/event/{eventId}/record/{recordId}/image/upload` (Upload up to 5 Record Images)
    - `PATCH /api/dashboard/event/{eventId}/record/{recordId}/image/update` (Reorder / Delete Record Images)
    - `DELETE /api/dashboard/event/{eventId}/record/{recordId}/delete` (Delete Record)
  - However, in `how-long-ago-client`, `RecordPrivateService` **only implements `GetRecordsPagination`**! Users have zero UI to add a record occurrence to an event, edit/delete records, upload record images, or like/dislike records!
- **What to Implement**:
  - **Backend Enrichment (`how-long-ago`)**:
    - Include `ImageId` (`ri.id`) and `ORDER BY ri.position ASC` in `ImageModelDTO` / `GetRecordsPaginationQueryHandler` and `GetRecordsPaginationPublicQueryHandler` so record images are ordered properly and their IDs are available to the client.
  - **Frontend (`how-long-ago-client`)**:
    - Complete `IRecordPrivateService` / `RecordPrivateService` with `CreateRecord`, `UpdateRecord`, `DeleteRecord`, `UpdateRecordLikeState`, and `UploadRecordImages`.
    - Add `CreateUpdateRecordModal.razor` and wire up **Create Record**, **Edit Record**, **Delete Record**, and **Upload Record Images** in `DashboardEvent.razor`.
    - Wire up **Like / Dislike Record** buttons on `EventPrivate.razor` (and distinct Like `HeartFill`/`HandThumbsUpFill` vs Dislike `HandThumbsDownFill` icons).

---

## Feature 3: User Profile & Shared Access Management (`AccountController` & `SharedUserController` Integration)
- **Why**:
  - Every Event card in the UI displays `Author: @item.UserInfo.FirstName @item.UserInfo.LastName` and avatar `@item.UserInfo.Image.ThumbnailHash`.
  - The backend already has `AccountController` (`GET /api/account/user-info`, `PUT /api/account/update`, `PATCH /api/account/image/update`, `GET /api/account/user-info-by-nickname`) and `SharedUserController` (`POST /api/dashboard/event/share/add`, `GET /api/dashboard/event/share/users`, `DELETE /api/dashboard/event/share/{sharedUserId}/delete`).
  - Yet the Blazor client has **no Account/Profile or Shared Users service or UI page**!
- **What to Implement**:
  - **Frontend (`how-long-ago-client`)**:
    - Create `IAccountPrivateService` / `AccountPrivateService` and `ISharedUserPrivateService` / `SharedUserPrivateService`.
    - Create a **Profile & Sharing Page** (`/profile` in `Pages/Private/Profile.razor` and sidebar link in `MainLayout.razor`) allowing authenticated users to:
      - View and update their `FirstName` and `LastName`.
      - Upload/update their profile avatar image.
      - Search users by name/nickname, add users to their Shared Access list, and remove shared users.

---

## Feature 4: SuperAdmin User Search & Pagination in `HowIdentity`
- **Why**:
  - `HowIdentity/Pages/SuperAdmin/AppUsers/Index.cshtml.cs` (Line 27) has `// TODO add pagination` and an `EmailFilter` property that is never passed to `SuperAdminUserService.GetUsers()`, plus a broken search form button that navigates to `/ServerSideSessions/Index`.
- **What to Implement**:
  - **Identity (`how-long-ago-identity`)**:
    - Update `ISuperAdminUserService.GetUsers(string emailFilter = null, int page = 1, int size = 20)` with parameterized Dapper filtering (`u.email ILIKE '%' || @EmailFilter || '%'`) and pagination.
    - Fix `Pages/SuperAdmin/AppUsers/Index.cshtml` and `Index.cshtml.cs` so admins can filter users by email and navigate pages cleanly.
