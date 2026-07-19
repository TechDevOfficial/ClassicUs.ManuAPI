---
sidebar_position: 1
---

# Loading Sprites

Embed a PNG as a resource and wrap it in a `LoadableSprite`. It caches the result but
retries automatically if Unity ever tears the asset down mid-match — the classic cause of
an ability icon quietly reverting to the native kill sprite.

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\saber_button.png">
    <LogicalName>saber_button.png</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

```csharp
internal static class VaderAssets
{
    private static readonly Assembly _assembly = typeof(VaderAssets).Assembly;
    private static readonly LoadableSprite _button = new(_assembly, "saber_button.png", 100f);

    public static Sprite LoadButton(Sprite original) => _button.Get() ?? original;
}
```

Falling back to `original` (the sprite you were handed, usually the native kill icon)
means your button still shows *something* even if the embedded asset fails to load —
never a blank sprite.
