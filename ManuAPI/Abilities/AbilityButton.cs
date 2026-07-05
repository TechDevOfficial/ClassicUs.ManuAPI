using System;
using TMPro;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public class AbilityButton
    {
        private readonly string _name;
        private readonly Func<Sprite, Sprite> _spriteFactory;
        private readonly Func<bool> _isVisible;
        private readonly Action _onClick;
        private readonly AspectPosition.EdgeAlignments _alignment;
        private readonly Vector3 _distanceFromEdge;

        private GameObject _buttonGo;
        private SpriteRenderer _renderer;
        private TextMeshPro _cooldownText;
        private PassiveButton _passiveButton;
        private float _cooldownRemaining;
        private float _effectRemaining;

        public Action OnEffectExpired;

        public AbilityButton(string name, Func<Sprite, Sprite> spriteFactory, Func<bool> isVisible, Action onClick,
            AspectPosition.EdgeAlignments alignment = AspectPosition.EdgeAlignments.LeftBottom,
            Vector3? distanceFromEdge = null)
        {
            _name = name;
            _spriteFactory = spriteFactory;
            _isVisible = isVisible;
            _onClick = onClick;
            _alignment = alignment;
            _distanceFromEdge = distanceFromEdge ?? new Vector3(1.4f, 1f, 0f);
        }

        public bool IsOnCooldown => _cooldownRemaining > 0f || _effectRemaining > 0f;

        public void StartCooldown(float seconds) => _cooldownRemaining = seconds;

        public void StartEffect(float seconds) => _effectRemaining = seconds;

        public void Tick(HudManager hud)
        {
            EnsureCreated(hud);
            if (_buttonGo == null) return;

            bool show = _isVisible();
            if (_buttonGo.activeSelf != show) _buttonGo.SetActive(show);
            if (!show) return;

            if (_effectRemaining > 0f)
            {
                _effectRemaining = Math.Max(0f, _effectRemaining - Time.fixedDeltaTime);
                if (_cooldownText != null)
                {
                    _cooldownText.text = Math.Ceiling(_effectRemaining).ToString("0");
                    _cooldownText.color = new Color(0.3f, 0.9f, 0.3f, 1f);
                }
                if (_renderer != null) _renderer.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                if (_effectRemaining <= 0f) OnEffectExpired?.Invoke();
            }
            else if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining = Math.Max(0f, _cooldownRemaining - Time.fixedDeltaTime);
                if (_cooldownText != null)
                {
                    _cooldownText.text = Math.Ceiling(_cooldownRemaining).ToString("0");
                    _cooldownText.color = Color.white;
                }
                if (_renderer != null) _renderer.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }
            else
            {
                if (_cooldownText != null)
                {
                    _cooldownText.text = string.Empty;
                    _cooldownText.color = Color.white;
                }
                if (_renderer != null) _renderer.color = Color.white;
            }
        }

        private void EnsureCreated(HudManager hud)
        {
            if (_buttonGo != null) return;
            if (hud == null || hud.KillButton == null) return;

            var killButton = hud.KillButton;
            var clusterContainer = killButton.gameObject.transform.parent;
            var clone = UnityEngine.Object.Instantiate(killButton.gameObject, hud.transform);
            clone.name = _name;

            var originalRenderer = clone.GetComponent<SpriteRenderer>();

            var clusterAnchor = clusterContainer != null ? clusterContainer.GetComponentInParent<AspectPosition>() : null;
            var aspectPosition = clone.GetComponent<AspectPosition>();
            if (aspectPosition == null) aspectPosition = clone.AddComponent<AspectPosition>();

            aspectPosition.parentCam = clusterAnchor != null ? clusterAnchor.parentCam : hud.UICamera;
            aspectPosition.Alignment = _alignment;
            aspectPosition.DistanceFromEdge = _distanceFromEdge;
            aspectPosition.updateAlways = true;
            aspectPosition.AdjustPosition();

            foreach (var comp in clone.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (comp == null) continue;
                if (comp.TryCast<PassiveButton>() != null) continue;
                if (comp.TryCast<TextMeshPro>() != null) continue;
                if (comp.TryCast<AspectPosition>() != null) continue;
                comp.enabled = false;
                UnityEngine.Object.Destroy(comp);
            }

            _buttonGo = clone;
            _renderer = clone.GetComponent<SpriteRenderer>();
            if (_renderer == null) _renderer = clone.AddComponent<SpriteRenderer>();

            var sprite = _spriteFactory != null ? _spriteFactory(originalRenderer != null ? originalRenderer.sprite : null) : null;
            if (sprite != null) _renderer.sprite = sprite;
            _renderer.color = Color.white;

            _cooldownText = clone.GetComponentInChildren<TextMeshPro>();

            _passiveButton = clone.GetComponentInChildren<PassiveButton>();
            if (_passiveButton != null && _passiveButton.OnClick != null)
            {
                _passiveButton.OnClick.RemoveAllListeners();
                _passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)HandleClick);
            }
        }

        private void HandleClick()
        {
            if (IsOnCooldown) return;
            _onClick?.Invoke();
        }

        public void Reset()
        {
            if (_buttonGo != null)
            {
                UnityEngine.Object.Destroy(_buttonGo);
                _buttonGo = null;
            }
            _renderer = null;
            _cooldownText = null;
            _passiveButton = null;
            _cooldownRemaining = 0f;
            _effectRemaining = 0f;
        }
    }
}
