using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
    public abstract class CustomAbility
    {
        private static readonly List<CustomAbility> _allAbilities = new();

        private AbilityButton _button;
        private readonly Vector3 _gridSlot;

        protected CustomAbility()
        {
            _allAbilities.Add(this);
            _gridSlot = AbilityButtonGrid.ReserveNextSlot();
        }

        internal static void ResetAll()
        {
            foreach (var ability in _allAbilities)
            {
                try { ability.Reset(); }
                catch (Exception e) { ManuAPIPlugin.Log.LogError("CustomAbility.ResetAll: " + e); }
            }
        }

        protected abstract string Name { get; }
        protected virtual float Cooldown => 0f;
        protected virtual AspectPosition.EdgeAlignments Alignment => AspectPosition.EdgeAlignments.LeftBottom;
        protected virtual Vector3 DistanceFromEdge => _gridSlot;

        protected abstract Sprite CreateIcon(Sprite original);
        protected abstract bool IsVisible();
        protected virtual bool CanActivate() => true;
        protected abstract void OnActivate();

        public void Tick(HudManager hud)
        {
            if (_button == null)
                _button = new AbilityButton(Name, CreateIcon, IsVisible, CanActivate, HandleClick, Alignment, DistanceFromEdge);

            _button.Tick(hud);
        }

        private void HandleClick()
        {
            OnActivate();
            if (Cooldown > 0f) _button.StartCooldown(Cooldown);
        }

        protected void StartEffect(float seconds) => _button?.StartEffect(seconds);

        protected void StartCooldown(float seconds) => _button?.StartCooldown(seconds);

        public void Reset()
        {
            _button?.Reset();
            _button = null;
        }
    }
}
