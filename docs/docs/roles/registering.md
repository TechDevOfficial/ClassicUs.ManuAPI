---
sidebar_position: 2
---

# Registering a Role

`RoleRegistry.Register` needs four things: your descriptor, a way to check whether the
IL2CPP type is ready yet, a way to trigger that registration, and the type itself once it
exists.

This looks fiddly because IL2CPP class injection is asynchronous — ManuAPI retries
automatically so you never have to poll for it yourself.

```csharp
public static bool IsTypeReady;
private static bool _classInjectorAttempted;

RoleRegistry.Register(new VaderRoleDescriptor(), () => IsTypeReady,
    EnsureIl2CppTypeRegistered, () => Il2CppType.Of<DarthVaderRole>());

public static void EnsureIl2CppTypeRegistered()
{
    if (_classInjectorAttempted) return;
    _classInjectorAttempted = true;

    ManactorAPI.RegisterIl2CppType(() =>
    {
        ClassInjector.RegisterTypeInIl2Cpp<DarthVaderRole>();
        IsTypeReady = true;
    });
}
```

:::info Your RoleBehaviour subclass stays thin
`DarthVaderRole` itself is a thin subclass of the native `RoleBehaviour` — it doesn't need
any ManuAPI-specific code. ManuAPI reads your `CustomRole` descriptor to fill in names,
colors, descriptions and team behaviour at runtime.
:::

## Assignment stays put

`RoleRegistry` also defends your assignment against the game's own role-sync flow: the
native `RoleManager.AssignRolesForTeam` call ends up sending a `PlayerControl.SetRole`
RPC that can arrive *after* your custom assignment and silently overwrite it. ManuAPI
detects that and reapplies your role immediately — you don't need to guard against it
yourself.
