---
sidebar_position: 3
---

# Lifecycle & Text

Override `OnAssigned` for anything that needs to happen the moment a player becomes your
role — showing UI, granting an ability, playing a sound.

```csharp
public override void OnAssigned(RoleBehaviour role, PlayerControl player)
{
    base.OnAssigned(role, player);
    // e.g. warm up ability state for this player
}
```

ManuAPI patches the intro cutscene, role descriptions, team color and kill-ability text so
your custom strings show up everywhere a native role's would — the intro splash, the task
list, the settings menu, the exile screen. You don't patch any of that yourself.
