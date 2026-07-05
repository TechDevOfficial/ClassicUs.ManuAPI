using System;
using System.Collections.Generic;

namespace ClassicUs.ManuAPI
{
    public static class RoleRegistry
    {
        private sealed class Handle
        {
            public CustomRole Descriptor;
            public Func<bool> IsClassReady;
            public Action EnsureClassRegistered;
            public Func<Il2CppSystem.Type> GetIl2CppType;
        }

        private static readonly List<Handle> _handles = new();

        public static void Register(CustomRole descriptor, Func<bool> isClassReady, Action ensureClassRegistered, Func<Il2CppSystem.Type> getIl2CppType)
        {
            _handles.Add(new Handle
            {
                Descriptor = descriptor,
                IsClassReady = isClassReady,
                EnsureClassRegistered = ensureClassRegistered,
                GetIl2CppType = getIl2CppType,
            });
        }

        private static bool _nativeTypesWarmedUp;

        internal static void WarmUpNativeRoleTypes()
        {
            if (_nativeTypesWarmedUp) return;
            if (!RoleManager.InstanceExists) return;
            var roles = RoleManager.Instance.allRoles;
            if (roles == null || roles.Count == 0) return;

            foreach (var r in roles)
            {
                if (r == null) continue;
                try { _ = r.AllOptions; }
                catch (Exception e) { ManuAPIPlugin.Log.LogError("WarmUpNativeRoleTypes: " + e); }
                _nativeTypesWarmedUp = true;
                break;
            }
        }

        internal static void EnsureAllTypesRegistered()
        {
            WarmUpNativeRoleTypes();

            for (int i = 0; i < _handles.Count; i++)
            {
                var h = _handles[i];

                if (!h.IsClassReady())
                {
                    h.EnsureClassRegistered();
                    continue;
                }

                if (h.Descriptor.IsRegisteredInRoleManager) continue;
                if (!RoleManager.InstanceExists) continue;

                TryAddRole(RoleManager.Instance, h);
            }
        }

        private static void TryAddRole(RoleManager rm, Handle h)
        {
            try
            {
                if (rm.allRoles != null)
                {
                    foreach (var r in rm.allRoles)
                    {
                        if (r == null || r.GetIl2CppType().Name != h.Descriptor.RoleTypeName) continue;
                        h.Descriptor.AssignedRoleName = r.roleCodeName;
                        return;
                    }
                }

                rm.AddRole(h.GetIl2CppType(), h.Descriptor.DisplayName);

                if (rm.allRoles == null) return;
                foreach (var role in rm.allRoles)
                {
                    if (role != null && role.GetIl2CppType().Name == h.Descriptor.RoleTypeName)
                    {
                        h.Descriptor.AssignedRoleName = role.roleCodeName;
                        ManuAPIPlugin.Log.LogInfo(h.Descriptor.RoleTypeName + " registered as role '" + role.roleCodeName + "'.");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("RoleRegistry: failed to register " + h.Descriptor.RoleTypeName + ": " + e);
            }
        }

        private static bool _loggedKillButtonOnce;

        internal static void UpdateKillButtonVisibility(HudManager hud)
        {
            try
            {
                if (hud == null || hud.KillButton == null) return;
                var local = PlayerControl.LocalPlayer;
                if (local == null || local.Data == null) return;
                var role = local.Data.myRole;
                if (role == null) return;

                if (!_loggedKillButtonOnce)
                {
                    _loggedKillButtonOnce = true;
                    ManuAPIPlugin.Log.LogInfo("UpdateKillButtonVisibility: first call with role=" + role.GetIl2CppType().Name);
                }

                for (int i = 0; i < _handles.Count; i++)
                {
                    var descriptor = _handles[i].Descriptor;
                    if (!descriptor.Matches(role) || !descriptor.CanUseKillButton) continue;

                    bool shouldShow = !local.Data.IsDead;
                    if (hud.KillButton.gameObject.activeSelf != shouldShow)
                        hud.KillButton.gameObject.SetActive(shouldShow);
                    return;
                }
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("UpdateKillButtonVisibility: " + e);
            }
        }

        internal static CustomRole Find(RoleBehaviour role)
        {
            if (role == null) return null;
            for (int i = 0; i < _handles.Count; i++)
                if (_handles[i].Descriptor.Matches(role)) return _handles[i].Descriptor;
            return null;
        }

        internal static IEnumerable<CustomRole> RegisteredCustomRoles()
        {
            for (int i = 0; i < _handles.Count; i++)
                if (_handles[i].Descriptor.IsRegisteredInRoleManager)
                    yield return _handles[i].Descriptor;
        }

        internal static void ApplyOnAssign(RoleBehaviour role, PlayerControl player)
        {
            for (int i = 0; i < _handles.Count; i++)
                if (_handles[i].Descriptor.Matches(role)) { _handles[i].Descriptor.Apply(role, player); return; }
        }

        internal static string ResolveEjectionText(GameData.PlayerInfo exiled)
        {
            if (exiled == null) return null;
            var role = exiled.myRole;
            if (role == null) return null;

            for (int i = 0; i < _handles.Count; i++)
                if (_handles[i].Descriptor.Matches(role)) return _handles[i].Descriptor.EjectionText(exiled.PlayerName);

            return null;
        }

        internal static void AssignForTeam(RoleManager rm, RoleTeamTypes type)
        {
            if (rm == null) return;
            var rng = new Random();

            for (int i = 0; i < _handles.Count; i++)
            {
                var h = _handles[i];
                if (h.Descriptor.TeamType != type) continue;
                if (!h.Descriptor.IsRegisteredInRoleManager) continue;

                AssignOne(rm, h.Descriptor, rng);
            }
        }

        private static void AssignOne(RoleManager rm, CustomRole descriptor, Random rng)
        {
            var candidates = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.Data == null || p.Data.Disconnected || p.Data.IsDead) continue;
                var role = p.Data.myRole;
                if (role == null || role.RoleTeamType != descriptor.TeamType) continue;
                candidates.Add(p);
            }

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int toAssign = Math.Min(descriptor.Count, candidates.Count);
            for (int i = 0; i < toAssign; i++)
            {
                if (rng.NextDouble() * 100.0 >= descriptor.RoleChancePercent) continue;
                rm.AssignRole(candidates[i], descriptor.AssignedRoleName);
            }
        }
    }
}
