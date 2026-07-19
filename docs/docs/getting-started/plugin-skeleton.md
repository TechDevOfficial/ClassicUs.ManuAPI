---
sidebar_position: 2
---

# Plugin Skeleton

This is the shape every ManuAPI mod starts from — everything else in these docs plugs
into this `Load()` method.

```csharp
[BepInPlugin(Guid, "My Mod", Version)]
[BepInDependency(ManactorPlugin.Guid)]
[BepInDependency(ManuAPIPlugin.Guid)]
public class MyModPlugin : BasePlugin
{
    public const string Guid = "yourname.mymod";
    public const string Version = "1.0.0";

    public override void Load()
    {
        ManactorAPI.Register("MyMod", Version);

        RoleRegistry.Register(new MyRoleDescriptor(), () => IsTypeReady,
            EnsureIl2CppTypeRegistered, () => Il2CppType.Of<MyRole>());

        ModBadgeAPI.RegisterLoadedModBadge("MyMod", Version, new Color(0.4f, 0.6f, 1f, 1f));

        new Harmony(Guid).PatchAll();
    }
}
```

Continue to [Custom Roles](/roles/custom-roles) to fill in `MyRoleDescriptor` and `MyRole`.
