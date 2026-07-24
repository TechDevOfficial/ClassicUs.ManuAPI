---
sidebar_position: 1
---

# Custom Game Modes

Create a `CustomGameMode` and register one instance from your plugin. The host selects
the mode with `GameModeRegistry.Select`; ManuAPI sends that selection through Manactor so
every modded client activates the same mode.

```csharp
public sealed class HideAndSeekMode : CustomGameMode
{
    public override string Id => "my.mod.hide-and-seek";
    public override string Name => "Hide and Seek";
    public override string Description => "Find the Crewmates before time runs out.";

    public override void OnGameStarted() { }
    public override void OnMeetingStarted(MeetingHud meeting) { }
}

public override void Load()
{
    GameModeRegistry.Register(new HideAndSeekMode());
}

// Host-only, for example from your own lobby UI:
GameModeRegistry.Select("my.mod.hide-and-seek");
```

## Lifecycle

`OnSelected` and `OnDeselected` track lobby selection. `OnGameStarted`, `OnHudUpdate`,
`OnMeetingStarted`, `OnMeetingEnded` and `OnGameEnded` cover the active match.

Set `OverrideVanillaWinConditions` to `true` only when your mode owns the win logic.
Return a `GameOverReason` from `CheckEndCriteria()` on the host to end the match, or call
`GameModeRegistry.EndGame(reason)` directly. Keep additional state synchronised with
Manactor RPCs; selection synchronisation does not automatically replicate mode data.
