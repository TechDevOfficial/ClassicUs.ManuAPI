using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public static class ModBadgeAPI
    {
        public readonly struct LoadedModBadge
        {
            public readonly string Text;
            public readonly Color Color;
            public LoadedModBadge(string text, Color color) { Text = text; Color = color; }
        }

        public readonly struct PrelobbyTag
        {
            public readonly string Label;
            public readonly string ColorHex;
            public PrelobbyTag(string label, string colorHex) { Label = label; ColorHex = colorHex; }
        }

        private static readonly List<LoadedModBadge> _badges = new();
        private static readonly List<PrelobbyTag> _tags = new();

        public static void RegisterLoadedModBadge(string modName, string version, Color color)
        {
            _badges.Add(new LoadedModBadge($"loaded {modName} v{version}", color));
        }

        public static void RegisterPrelobbyTag(string label, string colorHex = "#FFFFFF")
        {
            _tags.Add(new PrelobbyTag(label, colorHex));
        }

        internal static IReadOnlyList<LoadedModBadge> GetBadges() => _badges;
        internal static IReadOnlyList<PrelobbyTag> GetTags() => _tags;
    }

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    internal static class VersionShower_Start_Patch
    {
        private static void Postfix(VersionShower __instance)
        {
            try
            {
                var versionText = __instance == null ? null : __instance.text;
                if (versionText == null) return;

                var badges = ModBadgeAPI.GetBadges();
                if (badges.Count == 0) return;

                versionText.ForceMeshUpdate(false, false);
                var rend = versionText.GetComponent<MeshRenderer>();
                Bounds worldBounds = rend != null ? rend.bounds : new Bounds(versionText.transform.position, Vector3.zero);
                float gap = (worldBounds.size.y > 0f ? worldBounds.size.y : 0.3f) * 0.25f;
                float rightShift = (worldBounds.size.y > 0f ? worldBounds.size.y : 0.3f) * 0.23f;

                float baseY = worldBounds.min.y;
                for (int i = 0; i < versionText.transform.childCount; i++)
                {
                    var child = versionText.transform.GetChild(i);
                    if (child == null) continue;
                    if (!child.name.EndsWith("ModVersion") && !child.name.StartsWith("ManuAPIBadge")) continue;
                    var childRend = child.GetComponent<MeshRenderer>();
                    if (childRend != null) baseY = Mathf.Min(baseY, childRend.bounds.min.y);
                }

                for (int i = 0; i < badges.Count; i++)
                {
                    var name = "ManuAPIBadge_" + i;
                    if (versionText.transform.Find(name) != null) continue;

                    var go = new GameObject(name);
                    go.transform.SetParent(versionText.transform, true);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.position = new Vector3(
                        versionText.transform.position.x + rightShift,
                        baseY - gap * (i + 1),
                        versionText.transform.position.z);

                    var tmp = go.AddComponent<TextMeshPro>();
                    tmp.font = versionText.font;
                    tmp.fontSharedMaterial = versionText.fontSharedMaterial;
                    tmp.text = badges[i].Text;
                    tmp.fontSize = versionText.fontSize;
                    tmp.color = badges[i].Color;
                    tmp.alignment = versionText.alignment;
                    tmp.enableWordWrapping = false;
                }
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("ModBadgeAPI VersionShower patch: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    internal static class PingTracker_Update_Patch
    {
        private static void Postfix(PingTracker __instance)
        {
            try
            {
                if (!HudManager.InstanceExists) return;
                var tmp = HudManager.Instance.GameSettingsTMP;
                if (tmp == null || string.IsNullOrEmpty(tmp.text)) return;

                var tags = ModBadgeAPI.GetTags();
                for (int i = 0; i < tags.Count; i++)
                {
                    string marker = $"< {tags[i].Label} >";
                    if (tmp.text.Contains(marker)) continue;
                    tmp.text += $"\n<color={tags[i].ColorHex}>{marker}</color>";
                }
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("ModBadgeAPI PingTracker patch: " + e);
            }
        }
    }
}
