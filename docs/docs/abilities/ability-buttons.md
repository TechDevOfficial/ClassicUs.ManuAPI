---
sidebar_position: 1
---

# Ability Buttons

A `CustomAbility` clones the native kill button — same size, same cluster, same cooldown
ring — and hands you five things to fill in: a name, an icon, a visibility check, an
activation check, and what happens on click.

```csharp
internal class SaberAbility : CustomAbility
{
    protected override string Name => "SaberButton";
    protected override float Cooldown => VaderPlugin.ActiveCooldown;

    protected override Sprite CreateIcon(Sprite original) => VaderAssets.LoadButton(original);

    protected override bool IsVisible()
    {
        var local = PlayerControl.LocalPlayer;
        return VaderPlugin.IsVader(local) && !local.Data.IsDead;
    }

    protected override bool CanActivate()
    {
        var local = PlayerControl.LocalPlayer;
        return IsVisible() && !VaderSaberSystem.IsActive(local.Data.PlayerId);
    }

    protected override void OnActivate()
    {
        var local = PlayerControl.LocalPlayer;
        VaderSaberSystem.RequestStart(local.Data.PlayerId);
        StartEffect(VaderPlugin.ActiveDuration);
    }
}

internal static class SaberAbilityHolder
{
    private static readonly SaberAbility _ability = new();
    public static void Tick(HudManager hud) => _ability.Tick(hud);
}
```

Call `Tick(hud)` once per frame from a Harmony patch on `HudManager.FixedUpdate`:

```csharp
[HarmonyPatch(typeof(HudManager), nameof(HudManager.FixedUpdate))]
internal static class HudManager_FixedUpdate_Patch
{
    private static void Prefix(HudManager __instance) => SaberAbilityHolder.Tick(__instance);
}
```

:::tip You never reset the button yourself
`CustomAbility` registers itself automatically, and ManuAPI clears every registered
ability on death, restart and game-end — `HudManager.Start`, `AmongUsClient.OnGameEnd` /
`ExitGame`, `EndGameManager.NextGame` / `Exit`.
:::
