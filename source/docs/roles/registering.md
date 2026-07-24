---
sidebar_position: 2
---

# Registering a Role

For Classic Us 7.11+, use `RegisterVirtual` for ordinary custom roles. A virtual role
keeps a real vanilla role underneath it and overlays your name, intro text, colour and
ability behaviour. This is the safe default: it preserves native Impostor features such
as the kill button, vents and sabotage instead of replacing the entire `RoleBehaviour`.

```csharp
using ClassicUs.ManuAPI;

public override void Load()
{
    RoleRegistry.RegisterVirtual(new MyRoleDescriptor());
}

public static bool IsMyRole(PlayerControl player) =>
    RoleRegistry.IsAssigned(player, "my.mod.MyRole");
```

Use `CustomImpostorRole` when the role should keep Impostor abilities, and
`CustomCrewmateRole` when it should keep Crewmate behaviour. ManuAPI assigns the
appropriate native role before applying your descriptor, then synchronises the custom
identity to every modded client through Manactor.

## Native IL2CPP roles

`RoleRegistry.Register(...)` is still available for the rare case where you genuinely
need a custom IL2CPP `RoleBehaviour` subclass. It requires type injection and should only
be used when a virtual role cannot express the mechanic. Do not use it just to change
the role name, skin, intro, buttons or abilities.

## Assignment lifecycle

Custom assignments are cleared between matches. Meeting and match transitions keep the
underlying native role valid; do not repeatedly call `AssignCustomRole` every frame.
Listen to [Game Events](/events/game-events) for lifecycle changes instead.
