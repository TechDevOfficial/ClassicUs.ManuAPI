---
sidebar_position: 2
---

# Button Placement

Buttons anchor to the bottom-left corner like the native kill button. Override
`DistanceFromEdge` to place a second or third button next to the first —
`AbilityButtonGrid` holds the known-good slot positions so your buttons line up instead
of overlapping.

```csharp
protected override AspectPosition.EdgeAlignments Alignment => AspectPosition.EdgeAlignments.LeftBottom;
protected override Vector3 DistanceFromEdge => AbilityButtonGrid.SlotA;
```

| Slot | Distance from edge |
|---|---|
| `AbilityButtonGrid.DefaultSlot` | `(1.40, 1, 0)` — same spot as the native kill button |
| `AbilityButtonGrid.SlotA` | `(1.25, 1, 0)` — one button to the left |
| `AbilityButtonGrid.SlotB` | `(2.55, 1, 0)` — one button to the right |

:::caution
Don't compute slot positions dynamically from a shared counter across mods — button
order then depends on unpredictable static-initialization order between assemblies and
buttons will overlap. Pick a fixed slot per button instead.
:::
