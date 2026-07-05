using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public static class AssetUtils
    {
        public const int StandardIconSize = 512;

        public static byte[] ReadEmbeddedResource(Assembly assembly, string resourceName)
        {
            if (assembly == null || string.IsNullOrEmpty(resourceName)) return null;

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                ManuAPIPlugin.Log.LogError("Embedded resource not found: " + resourceName);
                return null;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public static Texture2D LoadTexture(byte[] pngOrJpgBytes)
        {
            if (pngOrJpgBytes == null) return null;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, pngOrJpgBytes);
            return texture;
        }

        public static Texture2D Resize(Texture2D source, int width, int height)
        {
            if (source == null || (source.width == width && source.height == height)) return source;

            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                float v = height <= 1 ? 0f : (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float u = width <= 1 ? 0f : (float)x / (width - 1);
                    result.SetPixel(x, y, source.GetPixelBilinear(u, v));
                }
            }
            result.Apply();
            return result;
        }

        public static Sprite CreateSprite(Texture2D texture, float pixelsPerUnit = 100f)
        {
            if (texture == null) return null;
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
        }

        public static Sprite LoadSpriteFromBytes(byte[] bytes, float pixelsPerUnit = 100f, int? resizeTo = null)
        {
            var texture = LoadTexture(bytes);
            if (texture == null) return null;
            if (resizeTo.HasValue) texture = Resize(texture, resizeTo.Value, resizeTo.Value);
            return CreateSprite(texture, pixelsPerUnit);
        }

        public static Sprite LoadSpriteFromEmbeddedResource(Assembly assembly, string resourceName, float pixelsPerUnit = 100f, int? resizeTo = null)
        {
            try
            {
                var bytes = ReadEmbeddedResource(assembly, resourceName);
                return bytes == null ? null : LoadSpriteFromBytes(bytes, pixelsPerUnit, resizeTo);
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("LoadSpriteFromEmbeddedResource (" + resourceName + "): " + e);
                return null;
            }
        }
    }
}
