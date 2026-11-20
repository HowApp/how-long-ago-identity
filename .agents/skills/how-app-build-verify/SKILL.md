---
name: how-app-build-verify
description: >-
  Builds and verifies all four .NET solutions/projects in the how_app workspace
  (how-long-ago-common, how-long-ago-identity, how-long-ago, how-long-ago-client)
  using local NuGet package feed or project references. Use when validating code
  changes, fixing compilation errors, or preparing commits across microservices.
---

# How App Build & Verification Skill

Use this skill to compile and verify all projects in `/home/mykhailo/dev/pet/how_app`.

## Build Order

Because `how-long-ago` and `how-long-ago-identity` depend on `HowCommon` (`how-long-ago-common`), always build and pack `how-long-ago-common` first if shared contracts or helpers were modified.

1. **`how-long-ago-common`**:
   ```bash
   dotnet build /home/mykhailo/dev/pet/how_app/how-long-ago-common/HowCommon.csproj
   ```
2. **`how-long-ago` (`How.sln`)**:
   ```bash
   dotnet build /home/mykhailo/dev/pet/how_app/how-long-ago/How.sln
   ```
3. **`how-long-ago-identity` (`HowIdentity.sln`)**:
   ```bash
   dotnet build /home/mykhailo/dev/pet/how_app/how-long-ago-identity/HowIdentity.sln
   ```
4. **`how-long-ago-client` (`HowClient.sln`)**:
   ```bash
   dotnet build /home/mykhailo/dev/pet/how_app/how-long-ago-client/HowClient.sln
   ```

## Local NuGet Feed for `HowCommon`
If the remote Azure DevOps feed is unavailable or `HowCommon` is updated locally:
1. Pack `HowCommon` into `/home/mykhailo/dev/pet/how_app/.local-nuget`:
   ```bash
   dotnet pack /home/mykhailo/dev/pet/how_app/how-long-ago-common/HowCommon.csproj -c Release -o /home/mykhailo/dev/pet/how_app/.local-nuget
   ```
2. Ensure `nuget.config` in `how-long-ago` and `how-long-ago-identity` includes the local feed source.
