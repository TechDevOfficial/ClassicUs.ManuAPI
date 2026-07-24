---
sidebar_position: 1
---

# Installation

This page starts from an empty folder. You need the **.NET 6 SDK**, a Classic Us 7.11
Windows installation with BepInEx, and the game opened at least once so BepInEx creates
its `interop` folder.

## 1. Create a project

Open PowerShell in the folder where you keep mods, then run:

```powershell
dotnet new classlib -n MyFirstClassicUsMod -f net6.0
cd MyFirstClassicUsMod
```

Replace the generated `.csproj` contents with this template. Change `GameDir` to your
own Classic Us folder.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyName>ClassicUs.MyFirstMod</AssemblyName>
    <RootNamespace>ClassicUs.MyFirstMod</RootNamespace>
    <LangVersion>latest</LangVersion>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <GameDir>C:\Path\To\Classic Us 2026.7.11 Windows</GameDir>
    <PluginsDir>$(GameDir)\BepInEx\plugins</PluginsDir>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ClassicUs.GameLibs" Version="2026.7.11.1" PrivateAssets="all" />
    <PackageReference Include="ClassicUs.Manactor" Version="1.1.0" PrivateAssets="all" ExcludeAssets="runtime" />
    <PackageReference Include="ClassicUs.ManuAPI" Version="1.5.1" PrivateAssets="all" ExcludeAssets="runtime" />
  </ItemGroup>

  <Target Name="CopyToPlugins" AfterTargets="Build">
    <MakeDir Directories="$(PluginsDir)" />
    <Copy SourceFiles="$(OutputPath)$(AssemblyName).dll" DestinationFolder="$(PluginsDir)" />
  </Target>
</Project>
```

## 2. Install the NuGet packages from the command line

Instead of editing XML yourself, you can add the packages with these commands:

```powershell
dotnet add package ClassicUs.GameLibs --version 2026.7.11.1
dotnet add package ClassicUs.Manactor --version 1.1.0
dotnet add package ClassicUs.ManuAPI --version 1.5.1
```

Then add the `GameDir`, `PluginsDir`, `AllowUnsafeBlocks` and `CopyToPlugins` parts from
the template above. `ExcludeAssets="runtime"` is important: Manactor and ManuAPI are
separate BepInEx plugins, so your mod must reference them at compile time without bundling
another copy into its output.

## 3. Build and install

```powershell
dotnet build -c Release
```

If the `CopyToPlugins` target is present, the build automatically places your DLL in
`BepInEx/plugins`. Start Classic Us and check `BepInEx/LogOutput.log` for your mod name.

:::tip Updating packages
Use `dotnet list package --outdated` to see available updates. Keep GameLibs matched to
the Classic Us game version you build against.
:::
