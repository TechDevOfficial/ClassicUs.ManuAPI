---
sidebar_position: 1
---

# Installation

This page starts from an empty folder. You need the **.NET 6 SDK**, a Classic Us 2026.8.17
Windows installation with BepInEx, and the game opened at least once so BepInEx creates
its `interop` folder.

## 1. Create a project

Open PowerShell in the folder where you keep mods, then run:

```powershell
dotnet new classlib -n MyFirstClassicUsMod -f net6.0
cd MyFirstClassicUsMod
```

Replace the generated `.csproj` contents with this template. It contains no personal
machine path; you pass the game folder only when you want to copy a finished DLL.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyName>ClassicUs.MyFirstMod</AssemblyName>
    <RootNamespace>ClassicUs.MyFirstMod</RootNamespace>
    <LangVersion>latest</LangVersion>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ClassicUs.GameLibs" Version="2026.8.17.1" PrivateAssets="all" />
    <PackageReference Include="ClassicUs.Manactor" Version="1.1.1" PrivateAssets="all" ExcludeAssets="runtime" />
    <PackageReference Include="ClassicUs.ManuAPI" Version="1.5.3" PrivateAssets="all" ExcludeAssets="runtime" />
  </ItemGroup>

  <Target Name="CopyToPlugins" AfterTargets="Build" Condition="'$(ClassicUsGameDir)' != ''">
    <MakeDir Directories="$(ClassicUsGameDir)\BepInEx\plugins" />
    <Copy SourceFiles="$(OutputPath)$(AssemblyName).dll" DestinationFolder="$(ClassicUsGameDir)\BepInEx\plugins" />
  </Target>
</Project>
```

## 2. Install the NuGet packages from the command line

Instead of editing XML yourself, you can add the packages with these commands:

```powershell
dotnet add package ClassicUs.GameLibs --version 2026.8.17.1
dotnet add package ClassicUs.Manactor --version 1.1.1
dotnet add package ClassicUs.ManuAPI --version 1.5.3
```

Then add `AllowUnsafeBlocks` and the optional `CopyToPlugins` target from the template
above. `ExcludeAssets="runtime"` is important: Manactor and ManuAPI are
separate BepInEx plugins, so your mod must reference them at compile time without bundling
another copy into its output.

## 3. Build and install

```powershell
# Build only
dotnet build -c Release

# Build and copy to the game on this computer
dotnet build -c Release -p:ClassicUsGameDir="C:\Path\To\Classic Us 2026.8.17 Windows"
```

When `ClassicUsGameDir` is supplied and the `CopyToPlugins` target is present, the build
automatically places your DLL in `BepInEx/plugins`. The path is a command-line setting,
so it never needs to be committed to GitHub. Start Classic Us and check
`BepInEx/LogOutput.log` for your mod name.

:::tip Updating packages
Use `dotnet list package --outdated` to see available updates. Keep GameLibs matched to
the Classic Us game version you build against.
:::
