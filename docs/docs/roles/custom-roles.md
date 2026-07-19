---
sidebar_position: 1
---

# Custom Roles

`ClassicUs.ManuAPI.CustomRole` is a plain C# class describing what a role looks like and
how it behaves — no manual IL2CPP boilerplate. Inherit from `CustomImpostorRole`,
`CustomCrewmateRole`, or `CustomRole` directly for neutral roles.

```csharp
internal class VaderRoleDescriptor : CustomImpostorRole
{
    public override string DisplayName => "Darth Vader";
    public override string RoleTypeName => "DarthVaderRole";
    public override int Count => 1;
    public override float RoleChancePercent => 100f;
    public override Color TeamColor => new(0.9f, 0.05f, 0.05f, 1f);
    public override string Description =>
        "You are Darth Vader. Spin your saber and cut down anyone who touches it.";
    public override string DescriptionShort => "Use the saber around you.";
    public override string EjectionText(string playerName) => $"{playerName} was Darth Vader.";
}
```

## Members

| Member | Purpose |
|---|---|
| `DisplayName` | Shown in the intro splash, task list and settings menu. |
| `RoleTypeName` | Must match the name of your IL2CPP-registered `RoleBehaviour` subclass. |
| `Count` / `RoleChancePercent` | How many candidates to pick, and the odds each one actually gets the role. |
| `CanVent` / `CanSabotage` / `CanUseKillButton` | Impostor roles default all three to `true`; crewmate roles default to `false`. |
| `TeamColor` | Overrides the name-tag and intro-screen color. |
| `EjectionText(name)` | Return `null` to fall back to the vanilla wording. |

`CustomImpostorRole` and `CustomCrewmateRole` set `TeamType` for you — you only override
it directly if you're building a neutral role on top of `CustomRole`.
