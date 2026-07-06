---
sidebar_position: 1
---

# Kill Manager

`KillManager.Kill` is a full re-implementation of the murder flow — it never calls the
native `RpcMurderPlayer` / kill-animation path at all, so every part of it is a plain flag
instead of a side effect you have to fight afterward.

```csharp
KillManager.Kill(owner, target, new KillRequest
{
    TeleportKiller = false,   // never touches the killer's position or movement
    CreateDeadBody = true,    // spawns a body with the victim's real player color
    PlayKillSound = true,
    ShowKillAnimation = true, // the victim's own close-up cutscene, purely cosmetic
});
```

| Option | Default | Effect |
|---|---|---|
| `TeleportKiller` | `true` | When `false`, the killer's position and movement are never touched — nothing to snap back. |
| `CreateDeadBody` | `true` | Spawns a body from the killer's own `KillAnimations` prefab, colored to match the victim. |
| `PlayKillSound` | `true` | Plays the killer's kill sound effect for the local client. |
| `ShowKillAnimation` | `true` | Shows the native kill overlay to the victim's own client only. |

:::info Networked automatically
Kills are host-authoritative — call `KillManager.Kill` from any client, and it routes
through Manactor's RPC system to the host before applying.
:::

## Why not just call `RpcMurderPlayer`?

The native kill flow always teleports the killer onto the body and plays a movement-lock
animation you can't opt out of piece by piece. `KillManager` spawns the body, marks the
victim dead, and plays sound/animation as independent steps — so a mod like a
lightsaber-swinging role can kill from a distance without dragging the killer across the
map.
