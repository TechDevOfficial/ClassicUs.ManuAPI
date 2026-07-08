---
sidebar_position: 2
---

# Loading Sounds

Embed a WAV file as a resource and wrap it in a `LoadableSound`. It caches the
decoded `AudioClip` and retries on the next `.Get()` call if loading ever
fails, the same self-healing behavior as `LoadableSprite`.

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\saber_hum.wav">
    <LogicalName>saber_hum.wav</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

```csharp
internal static class VaderAssets
{
    private static readonly Assembly _assembly = typeof(VaderAssets).Assembly;
    private static readonly LoadableSound _saberHum = new(_assembly, "saber_hum.wav");

    public static AudioClip LoadSaberHum() => _saberHum.Get();
}
```

```csharp
SoundManager.Instance.PlaySound(VaderAssets.LoadSaberHum(), false, 1f);
```

:::note WAV only
`LoadableSound` decodes 16-bit PCM WAV data directly, with no dependency on
Unity's `UnityWebRequest` audio importer. Export your clips as 16-bit PCM
`.wav` — MP3/OGG embedded resources are not supported.
:::

Continue to [3D Positional Audio](/assets/spatial-audio) to play a clip at a
world position instead of globally.
