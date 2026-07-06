using System;
using System.Collections.Generic;
using ClassicUs.Manactor;

namespace ClassicUs.ManuAPI
{
    public static class RoleRegistry
    {
        private const string RpcAssignCustomRoleKey = "classicus.manuapi.AssignCustomRole";
        private static readonly List<(byte playerId, string roleTypeName)> _pendingAssignments = new();
        private static readonly Dictionary<byte, string> _assignedCustomRoles = new();

        private sealed class Handle
        {
            public CustomRole Descriptor;
            public Func<bool> IsClassReady;
            public Action EnsureClassRegistered;
            public Func<Il2CppSystem.Type> GetIl2CppType;
            public RoleBehaviour RegisteredRole;
            public bool AddAttempted;
        }

        private static readonly List<Handle> _handles = new();

        internal static void RegisterNetworkHandlers()
        {
            ManactorAPI.RegisterRpcMethods(typeof(RoleRegistry));
        }

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

                if (!RoleManager.InstanceExists) continue;
                if (h.RegisteredRole != null || h.Descriptor.IsRegisteredInRoleManager) continue;

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
                        if (!MatchesRegisteredRole(r, h.Descriptor)) continue;
                        h.RegisteredRole = r;
                        h.Descriptor.AssignedRoleName = r.roleCodeName;
                        return;
                    }
                }

                h.AddAttempted = true;
                rm.AddRole(h.GetIl2CppType(), h.Descriptor.DisplayName);

                if (rm.allRoles == null) return;
                foreach (var role in rm.allRoles)
                {
                    if (MatchesRegisteredRole(role, h.Descriptor))
                    {
                        h.RegisteredRole = role;
                        h.Descriptor.AssignedRoleName = role.roleCodeName;
                        ManuAPIPlugin.Log.LogInfo(h.Descriptor.RoleTypeName + " registered as role '" + role.roleCodeName + "'.");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message != null && e.Message.Contains("already exists"))
                {
                    var existing = FindRegisteredRole(h.Descriptor);
                    if (existing != null)
                    {
                        h.RegisteredRole = existing;
                        h.Descriptor.AssignedRoleName = existing.roleCodeName;
                        return;
                    }

                    if (string.IsNullOrEmpty(h.Descriptor.AssignedRoleName))
                        h.Descriptor.AssignedRoleName = h.Descriptor.RoleTypeName;
                    h.AddAttempted = true;
                    ManuAPIPlugin.Log.LogWarning("RoleRegistry: " + h.Descriptor.RoleTypeName + " already exists; keeping role name '" + h.Descriptor.AssignedRoleName + "' and not retrying registration.");
                    return;
                }

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

        public static void ClearRuntimeAssignments()
        {
            _pendingAssignments.Clear();
            _assignedCustomRoles.Clear();
        }

        internal static void ProcessPendingAssignments()
        {
            ReapplyAssignedCustomRoles();

            if (_pendingAssignments.Count == 0) return;
            if (!RoleManager.InstanceExists) return;

            EnsureAllTypesRegistered();

            for (int i = _pendingAssignments.Count - 1; i >= 0; i--)
            {
                var pending = _pendingAssignments[i];
                var descriptor = FindDescriptor(pending.roleTypeName);
                var player = FindPlayer(pending.playerId);
                if (descriptor == null || player == null) continue;

                if (AssignCustomRole(player, descriptor, false, false))
                    _pendingAssignments.RemoveAt(i);
            }
        }

        private static void ReapplyAssignedCustomRoles()
        {
            if (_assignedCustomRoles.Count == 0) return;
            if (!RoleManager.InstanceExists) return;

            foreach (var entry in _assignedCustomRoles)
            {
                var descriptor = FindDescriptor(entry.Value);
                var player = FindPlayer(entry.Key);
                if (descriptor == null || player == null || player.Data == null) continue;
                if (player.Data.myRole != null && descriptor.Matches(player.Data.myRole)) continue;

                AssignCustomRole(player, descriptor, false, false);
            }
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

                EnsureAllTypesRegistered();
                if (FindRegisteredRole(h.Descriptor) == null && !h.Descriptor.IsRegisteredInRoleManager)
                {
                    TryAddRole(rm, h);
                    if (FindRegisteredRole(h.Descriptor) == null)
                    {
                        ManuAPIPlugin.Log.LogWarning("RoleRegistry: skipping " + h.Descriptor.RoleTypeName + " assignment; role is not registered in current RoleManager.");
                        continue;
                    }
                }

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
                if (role == null)
                {
                    if (descriptor.TeamType != RoleTeamTypes.Crewmate) continue;
                    candidates.Add(p);
                    continue;
                }

                if (role.RoleTeamType != descriptor.TeamType) continue;
                candidates.Add(p);
            }

            ManuAPIPlugin.Log.LogInfo("RoleRegistry: " + descriptor.RoleTypeName + " candidates=" + candidates.Count +
                                      " count=" + descriptor.Count + " chance=" + descriptor.RoleChancePercent + ".");

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int toAssign = Math.Min(descriptor.Count, candidates.Count);
            for (int i = 0; i < toAssign; i++)
            {
                if (rng.NextDouble() * 100.0 >= descriptor.RoleChancePercent) continue;
                if (AssignCustomRole(candidates[i], descriptor, true))
                    ManactorAPI.SendRpcMethod(RpcAssignCustomRoleKey, candidates[i].Data.PlayerId, descriptor.RoleTypeName);
            }
        }

        [ManactorRpc(RpcAssignCustomRoleKey)]
        private static void OnAssignCustomRoleRpc(byte senderId, byte playerId, string roleTypeName)
        {
            var client = AmongUsClient.Instance;
            if (client != null && client.AmHost) return;

            var descriptor = FindDescriptor(roleTypeName);
            if (descriptor == null)
            {
                ManuAPIPlugin.Log.LogError("RoleRegistry: received assignment for unknown custom role " + roleTypeName + ".");
                return;
            }

            var player = FindPlayer(playerId);
            if (player == null)
            {
                QueuePendingAssignment(playerId, roleTypeName);
                return;
            }

            if (!AssignCustomRole(player, descriptor, false, false))
                QueuePendingAssignment(playerId, roleTypeName);
        }

        internal static void ReapplyIfCustomRole(PlayerControl player)
        {
            if (player == null || player.Data == null) return;
            if (!_assignedCustomRoles.TryGetValue(player.Data.PlayerId, out var roleTypeName)) return;

            var role = player.Data.myRole;
            if (role != null && role.GetIl2CppType().Name == roleTypeName) return;

            var descriptor = FindDescriptor(roleTypeName);
            if (descriptor == null) return;

            ManuAPIPlugin.Log.LogWarning("RoleRegistry: native SetRole reverted " + player.Data.PlayerName +
                                          " away from " + roleTypeName + "; reapplying.");
            AssignCustomRole(player, descriptor, false, false);
        }

        private static void QueuePendingAssignment(byte playerId, string roleTypeName)
        {
            for (int i = 0; i < _pendingAssignments.Count; i++)
            {
                var pending = _pendingAssignments[i];
                if (pending.playerId == playerId && pending.roleTypeName == roleTypeName)
                    return;
            }

            _pendingAssignments.Add((playerId, roleTypeName));
            ManuAPIPlugin.Log.LogInfo("RoleRegistry: queued custom role sync " + roleTypeName + " for player " + playerId + ".");
        }

        private static bool AssignCustomRole(PlayerControl player, CustomRole descriptor, bool isHostAssignment, bool logFailures = true)
        {
            if (player == null || player.Data == null || descriptor == null) return false;

            var role = FindRegisteredRole(descriptor);
            if (role == null)
            {
                if (!descriptor.IsRegisteredInRoleManager)
                    EnsureAllTypesRegistered();
                role = FindRegisteredRole(descriptor);
                if (role == null)
                {
                    if (logFailures)
                        ManuAPIPlugin.Log.LogError("RoleRegistry: registered role instance not found for " + descriptor.RoleTypeName + ".");
                    return false;
                }
            }

            player.Data.myRole = role;
            player.Data.roleWhenAliveName = role.roleCodeName;
            role.Player = player;
            role.OnAssign(player);
            _assignedCustomRoles[player.Data.PlayerId] = descriptor.RoleTypeName;

            ManuAPIPlugin.Log.LogInfo((isHostAssignment ? "Assigned" : "Synced") + " custom role " +
                                      descriptor.RoleTypeName + " to " + player.Data.PlayerName + ".");
            return true;
        }

        private static CustomRole FindDescriptor(string roleTypeName)
        {
            for (int i = 0; i < _handles.Count; i++)
            {
                var descriptor = _handles[i].Descriptor;
                if (descriptor != null && descriptor.RoleTypeName == roleTypeName)
                    return descriptor;
            }

            return null;
        }

        private static PlayerControl FindPlayer(byte playerId)
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p != null && p.Data != null && p.Data.PlayerId == playerId)
                    return p;
            }

            return null;
        }

        private static RoleBehaviour FindRegisteredRole(CustomRole descriptor)
        {
            for (int i = 0; i < _handles.Count; i++)
            {
                var h = _handles[i];
                if (h.Descriptor != descriptor || h.RegisteredRole == null) continue;
                return h.RegisteredRole;
            }

            if (!RoleManager.InstanceExists) return null;
            var roles = RoleManager.Instance.allRoles;
            if (roles == null) return null;

            foreach (var role in roles)
            {
                if (MatchesRegisteredRole(role, descriptor))
                {
                    CacheRegisteredRole(descriptor, role);
                    return role;
                }
            }

            return null;
        }

        private static void CacheRegisteredRole(CustomRole descriptor, RoleBehaviour role)
        {
            for (int i = 0; i < _handles.Count; i++)
            {
                var h = _handles[i];
                if (h.Descriptor != descriptor) continue;
                h.RegisteredRole = role;
                return;
            }
        }

        private static bool MatchesRegisteredRole(RoleBehaviour role, CustomRole descriptor)
        {
            if (role == null || descriptor == null) return false;

            try
            {
                var type = role.GetIl2CppType();
                if (type != null && type.Name == descriptor.RoleTypeName) return true;
            }
            catch { }

            try
            {
                if (!string.IsNullOrEmpty(role.roleCodeName) &&
                    (role.roleCodeName == descriptor.RoleTypeName || role.roleCodeName == descriptor.AssignedRoleName ||
                     role.roleCodeName == descriptor.DisplayName))
                    return true;
            }
            catch { }

            return false;
        }
    }
}
