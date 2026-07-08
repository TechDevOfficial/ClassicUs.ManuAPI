---
sidebar_position: 1
---

# Settings Menu

Register a block of rows once; ManuAPI injects them right after the vanilla options and
repositions everything using the game's own layout pass.

```csharp
SettingsMenuAPI.Register(3, builder =>
{
    builder.AddToggle("VaderEnabled", "Enable Darth Vader",
        () => _cfgEnabled.Value,
        val => { _cfgEnabled.Value = val; Save(); });

    builder.AddNumeric("VaderCount", "Darth Vader Count", 1f, 0f, 3f, "0",
        () => _cfgCount.Value,
        val => { _cfgCount.Value = (int)val; Save(); });

    builder.ExpandScroller(3f);
});
```

| Method | Use for |
|---|---|
| `AddToggle(key, label, getter, setter)` | On/off options. |
| `AddNumeric(key, label, step, min, max, format, getter, setter)` | Sliders — counts, percentages, durations. |
| `ExpandScroller(rows)` | Call once at the end with the total row count you added, so the menu scrolls far enough to show them all. |
