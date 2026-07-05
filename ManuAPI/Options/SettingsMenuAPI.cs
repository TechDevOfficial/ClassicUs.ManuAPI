using System;
using System.Collections.Generic;
using ClassicUs.Manactor;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ClassicUs.ManuAPI
{
    public class SettingsMenuBuilder
    {
        private readonly GameSettingMenu _menu;
        private readonly Transform _parent;
        private readonly NumberOption _template;
        private readonly bool _isHost;
        private readonly int _baseItemCount;
        private int _row;

        internal SettingsMenuBuilder(GameSettingMenu menu, Transform parent, NumberOption template, int startRow)
        {
            _menu = menu;
            _parent = parent;
            _template = template;
            _baseItemCount = menu.AllItems?.Count ?? 0;
            _row = startRow;
            _isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
        }

        public TextMeshPro AddToggle(string name, string label, Func<bool> getter, Action<bool> setter)
        {
            var (target, valueText) = GetOrCreate(name, label);
            _row++;

            if (valueText != null) valueText.text = getter() ? "On" : "Off";

            foreach (var pb in target.GetComponentsInChildren<PassiveButton>())
            {
                if (pb == null) continue;
                pb.gameObject.SetActive(_isHost);
                if (!_isHost || pb.OnClick == null) continue;
                pb.OnClick.RemoveAllListeners();
                var capturedText = valueText;
                pb.OnClick.AddListener((UnityAction)(() =>
                {
                    setter(!getter());
                    if (capturedText != null) capturedText.text = getter() ? "On" : "Off";
                }));
            }

            return valueText;
        }

        public TextMeshPro AddNumeric(string name, string label, float step, float min, float max, string format, Func<float> getter, Action<float> setter)
        {
            var (target, valueText) = GetOrCreate(name, label);
            _row++;

            if (valueText != null) valueText.text = getter().ToString(format);

            var buttons = target.GetComponentsInChildren<PassiveButton>();
            var sorted = new List<PassiveButton>();
            foreach (var b in buttons) if (b != null) sorted.Add(b);
            sorted.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));

            foreach (var pb in sorted) pb.gameObject.SetActive(_isHost);

            if (_isHost && sorted.Count >= 2)
            {
                var dec = sorted[0];
                var inc = sorted[sorted.Count - 1];
                var capturedText = valueText;

                dec.OnClick.RemoveAllListeners();
                dec.OnClick.AddListener((UnityAction)(() =>
                {
                    float val = Math.Max(min, getter() - step);
                    setter(val);
                    if (capturedText != null) capturedText.text = getter().ToString(format);
                }));

                inc.OnClick.RemoveAllListeners();
                inc.OnClick.AddListener((UnityAction)(() =>
                {
                    float val = Math.Min(max, getter() + step);
                    setter(val);
                    if (capturedText != null) capturedText.text = getter().ToString(format);
                }));
            }

            return valueText;
        }

        public void ExpandScroller(float extraRows)
        {
            var scroller = _parent.GetComponentInParent<Scroller>();
            if (scroller == null || scroller.YBounds == null) return;
            var yb = scroller.YBounds;
            scroller.YBounds = new FloatRange(yb.min, yb.max + extraRows);
        }

        private (Transform target, TextMeshPro valueText) GetOrCreate(string name, string label)
        {
            var existing = _parent.Find(name);
            if (existing != null)
            {
                float yPos = _menu.YStart - (_baseItemCount + _row) * _menu.YOffset;
                existing.localPosition = new Vector3(existing.localPosition.x, yPos, existing.localPosition.z);
                EnsureTracked(existing);
                return (existing, existing.GetComponentInChildren<TextMeshPro>());
            }

            var go = UnityEngine.Object.Instantiate(_template.gameObject, _parent);
            go.name = name;
            float y = _menu.YStart - (_baseItemCount + _row) * _menu.YOffset;
            go.transform.localPosition = new Vector3(_template.transform.localPosition.x, y, _template.transform.localPosition.z);
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(true);

            var no = go.GetComponent<NumberOption>();
            var titleText = no != null ? no.TitleText : null;
            var valueText = no != null ? no.ValueText : null;
            if (titleText != null) titleText.text = label;
            if (no != null) UnityEngine.Object.Destroy(no);

            EnsureTracked(go.transform);
            return (go.transform, valueText);
        }

        private void EnsureTracked(Transform item)
        {
            if (item == null || _menu.AllItems == null) return;
            for (int i = 0; i < _menu.AllItems.Count; i++)
            {
                if (_menu.AllItems[i] == item)
                    return;
            }

            _menu.AllItems.Add(item);
        }
    }

    public static class SettingsMenuAPI
    {
        private sealed class Registration
        {
            public int RowCount;
            public Action<SettingsMenuBuilder> Build;
        }

        private static readonly List<Registration> _registrations = new();

        public static void Register(int rowCount, Action<SettingsMenuBuilder> build)
        {
            _registrations.Add(new Registration { RowCount = rowCount, Build = build });
        }

        internal static void BuildAll(GameSettingMenu menu)
        {
            if (menu == null || menu.AllItems == null || menu.AllItems.Count == 0) return;
            var parent = menu.AllItems[0].parent;
            if (parent == null) return;
            var template = menu.keyvaluePrefab;
            if (template == null) return;

            for (int i = 0; i < _registrations.Count; i++)
            {
                var reg = _registrations[i];
                int start = ManactorAPI.ReserveSettingsRows(menu.GetInstanceID(), reg.RowCount);
                var builder = new SettingsMenuBuilder(menu, parent, template, start);
                try { reg.Build(builder); }
                catch (Exception e) { ManuAPIPlugin.Log.LogError("SettingsMenuAPI build failed: " + e); }
            }

            try { menu.RepositionChildren(); }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("SettingsMenuAPI RepositionChildren: " + e); }
        }
    }

    [HarmonyPatch(typeof(SettingMenu), nameof(SettingMenu.OnEnable))]
    internal static class SettingMenu_OnEnable_Patch
    {
        private static void Postfix(SettingMenu __instance)
        {
            var gameMenu = __instance.TryCast<GameSettingMenu>();
            if (gameMenu == null) return;
            try { SettingsMenuAPI.BuildAll(gameMenu); }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("SettingsMenuAPI.BuildAll: " + e); }
        }
    }
}
