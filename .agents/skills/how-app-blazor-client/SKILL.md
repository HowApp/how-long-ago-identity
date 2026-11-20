---
name: how-app-blazor-client
description: >-
  Guides development of Blazor WebAssembly pages, modals, SignalR hubs, and API
  client services in how-long-ago-client (HowClient). Use when adding UI features,
  modals, or integrating backend API endpoints in the Blazor frontend.
---

# How App Blazor WebAssembly Client Skill

Use this skill when modifying `how-long-ago-client`.

## Key Patterns
1. **API Clients**:
   - `AuthorizedClientAPI`: Automatically attaches Bearer access tokens via `AuthorizationMessageHandler`. Used by services in `Services/Private/`.
   - `AnonymousClientAPI`: Used by services in `Services/Public/`.
   - Always check `if (response.Failed)` and notify via `IInternalNotificationService.NotifyError(response.ToString())`.
2. **Modals (`Blazor.Bootstrap`)**:
   - When opening dynamic modals via `_modal.ShowAsync<TModal>(title, parameters)`, parameter names in `Parameterention` / `Dictionary<string, object>` MUST match the exact property name on `TModal` (e.g., `"OnClickCallback"`).
3. **Lifecycle & Timers**:
   - Live duration timers (`System.Timers.Timer`) and `InternalNotificationService.OnError` subscriptions must be disposed in `Dispose()`.
   - Always guard `StateHasChanged()` with `await InvokeAsync(StateHasChanged)` in background timer callbacks.
