---
sidebar_position: 3
---

# 3D Positional Audio

`SpatialAudio` plays a clip at a world position using Unity's real 3D audio
engine — players close to the source hear it clearly, players farther than
`maxDistance` don't hear it at all. There's no custom volume math to get
wrong: it spawns a temporary `AudioSource` with `spatialBlend = 1` and a
linear rolloff, and lets Unity's own listener-distance attenuation do the
work against whichever `AudioListener` is active (the local player's camera).

```csharp
SpatialAudio.PlayAt(VaderAssets.LoadSaberHum(), owner.transform.position, maxDistance: 6f);
```

## Attaching to a moving object

For a sound that should follow a player or object around (footsteps, an aura
hum), attach it to a transform instead of a fixed point:

```csharp
SpatialAudio.PlayAttachedTo(VaderAssets.LoadSaberHum(), owner.transform, maxDistance: 6f);
```

## API

| Member | Purpose |
|---|---|
| `PlayAt(clip, position, maxDistance, volume)` | Plays a one-shot clip at a fixed world position. |
| `PlayAttachedTo(clip, target, maxDistance, volume)` | Plays a one-shot clip parented to a transform, following it until the clip ends. |
| `IsAudibleFrom(listenerPos, sourcePos, maxDistance)` | A plain distance check, useful if you want to gate other logic (e.g. only show a caption) the same way the audio itself is gated. |

Both playback methods clean up their temporary `GameObject` automatically
once the clip finishes — you don't manage the `AudioSource`'s lifetime
yourself.

:::tip Distance tuning
`maxDistance` defaults to 8 units — roughly a couple of rooms on most Classic
Us maps. Pass a smaller value for a quiet, personal effect (a whispered
ability cue) or a larger one for something meant to alarm nearby crewmates.
:::
