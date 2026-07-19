---
sidebar_position: 1
---

# Installation

Add both packages to your mod's `.csproj`. ManuAPI carries Manactor as a dependency, but
pin both explicitly so builds stay reproducible.

```xml
<ItemGroup>
  <PackageReference Include="ClassicUs.GameLibs" Version="2026.7.4.1" PrivateAssets="all" />
  <PackageReference Include="ClassicUs.Manactor" Version="1.0.26" PrivateAssets="all" ExcludeAssets="runtime" />
  <PackageReference Include="ClassicUs.ManuAPI" Version="1.3.21" PrivateAssets="all" ExcludeAssets="runtime" />
</ItemGroup>
```

`ExcludeAssets="runtime"` keeps the DLL out of your own build output — Manactor and
ManuAPI are loaded as their own BepInEx plugins, side by side with yours, so bundling a
second copy would just create duplicate types at runtime.

:::note Local packages during development
If you're building ManuAPI or Manactor from source alongside your mod, `dotnet pack` them
into a local `nupkg` folder and point a `nuget.config` at it with a `<packageSources>`
entry — no need to publish anywhere while iterating.
:::
