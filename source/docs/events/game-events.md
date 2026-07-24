---
sidebar_position: 1
---

# Game Events

`GameEvents` exposes common match actions that mods normally patch themselves. Subscribe
during `Load`; each handler is isolated, so an exception in one mod is logged without
breaking the game loop.

```csharp
public override void Load()
{
    GameEvents.AfterMurder += OnMurder;
    GameEvents.AtMeeting += _ => ResetRoundState();
}

private static void OnMurder(MurderEventArgs e)
{
    // e.Killer and e.Target are PlayerControl instances.
}
```

| Event | When it runs |
|---|---|
| `BeforeMurder` / `AfterMurder` | Immediately before or after `MurderPlayer`. Set `Cancelled` in `BeforeMurder` to block it. |
| `BeforeReport` / `AfterReport` | A body report or emergency button press. `IsEmergencyMeeting` identifies the latter. |
| `AtMeeting` / `AfterMeeting` | Meeting HUD opens or closes. |
| `PlayerEnteredVent` / `PlayerExitedVent` | A player enters or exits a vent. |
| `TaskCompleted`, `PlayerExiled` | A task completes or a player is exiled. |
| `GameStarted`, `GameEnded` | The active game begins or ends. |
| `PlayerJoined`, `PlayerLeft` | Lobby connection changes; `PlayerLeft` includes the disconnect reason. |

Events are local callbacks, not RPCs. Use Manactor RPCs whenever custom state must be
replicated to other clients.
