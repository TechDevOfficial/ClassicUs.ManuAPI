using System;
using System.Collections.Generic;
using ClassicUs.Manactor;
using HarmonyLib;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
    internal static class IntroText
    {
        internal static void ApplyCurrent()
        {
            try
            {
                var intro = IntroCutscene.Instance;
                if (intro == null || !intro.gameObject.activeInHierarchy || intro.Title == null || intro.DescriptionText == null)
                    return;

                var local = PlayerControl.LocalPlayer;
                if (local == null || local.Data == null) return;

                var descriptor = RoleRegistry.Find(local.Data.myRole) ?? RoleRegistry.FindAssigned(local);
                if (descriptor == null) return;

                intro.Title.text = descriptor.DisplayName;
                intro.Title.color = descriptor.TeamColor;
                intro.DescriptionText.gameObject.SetActive(true);
                intro.DescriptionText.enabled = true;
                intro.DescriptionText.color = Color.white;
                intro.DescriptionText.text = descriptor.DescriptionShort;
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("Apply custom intro text: " + e);
            }
        }
    }

    internal static class RolePatchHelper
    {
        internal static bool TryCustomString(RoleBehaviour role, Func<CustomRole, string> getValue, ref string result)
        {
            if (role == null) return true;

            var d = RoleRegistry.Find(role);
            if (d != null)
            {
                result = getValue(d);
                return false;
            }

            if (!IsVanillaRole(role))
            {
                result = string.Empty;
                return false;
            }

            return true;
        }

        private static bool IsVanillaRole(RoleBehaviour role)
        {
            switch (role.GetIl2CppType().Name)
            {
                case "CrewmateRole":
                case "ImpostorRole":
                case "CrewmateGhostRole":
                case "ImpostorGhostRole":
                case "NeutralGhostRole":
                    return true;
                default:
                    return false;
            }
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.roleDisplayName), MethodType.Getter)]
    internal static class RoleBehaviour_DisplayName_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref string __result) =>
            RolePatchHelper.TryCustomString(__instance, d => d.DisplayName, ref __result);
    }

    [HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.roleDisplayName), MethodType.Getter)]
    internal static class ImpostorRole_DisplayName_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref string __result) =>
            RolePatchHelper.TryCustomString(__instance, d => d.DisplayName, ref __result);
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.roleDescription), MethodType.Getter)]
    internal static class RoleBehaviour_Description_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref string __result) =>
            RolePatchHelper.TryCustomString(__instance, d => d.Description, ref __result);
    }

    [HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.roleDescription), MethodType.Getter)]
    internal static class ImpostorRole_Description_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref string __result) =>
            RolePatchHelper.TryCustomString(__instance, d => d.Description, ref __result);
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.roleDescriptionShort), MethodType.Getter)]
    internal static class RoleBehaviour_DescriptionShort_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref string __result) =>
            RolePatchHelper.TryCustomString(__instance, d => d.DescriptionShort, ref __result);
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.TeamColor), MethodType.Getter)]
    internal static class RoleBehaviour_TeamColor_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref Color __result)
        {
            var d = RoleRegistry.Find(__instance);
            if (d == null) return true;
            __result = d.TeamColor;
            return false;
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.IntroSound), MethodType.Getter)]
    internal static class RoleBehaviour_IntroSound_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref AudioClip __result)
        {
            var d = RoleRegistry.Find(__instance);
            if (d == null) return true;
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.KillAbilityName), MethodType.Getter)]
    internal static class RoleBehaviour_KillAbilityName_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref string __result)
        {
            var d = RoleRegistry.Find(__instance);
            if (d == null) return true;
            __result = d.KillAbilityName;
            return false;
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.KillAbilityImageName), MethodType.Getter)]
    internal static class RoleBehaviour_KillAbilityImageName_Patch
    {
        private static bool Prefix(RoleBehaviour __instance, ref string __result)
        {
            var d = RoleRegistry.Find(__instance);
            if (d == null) return true;
            __result = d.KillAbilityImageName;
            return false;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.GetTeamColor))]
    internal static class IntroCutscene_GetTeamColor_Patch
    {
        private static void Postfix(RoleBehaviour role, ref Color __result)
        {
            var d = RoleRegistry.Find(role);
            if (d != null) __result = d.TeamColor;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._BeginTeam_d__35), nameof(IntroCutscene._BeginTeam_d__35.MoveNext))]
    internal static class IntroCutscene_BeginTeam_MoveNext_Patch
    {
        private static void Postfix(IntroCutscene._BeginTeam_d__35 __instance, ref bool __result)
        {
            if (!__result || __instance == null || __instance.__4__this == null) return;

            var local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null || local.Data.myRole == null) return;

            var d = RoleRegistry.Find(local.Data.myRole) ?? RoleRegistry.FindAssigned(local);
            if (d == null) return;

            __instance.__4__this.Title.text = d.DisplayName;
            __instance.__4__this.Title.color = d.TeamColor;
            __instance.__4__this.DescriptionText.text = d.DescriptionShort;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._RetryTaskTextWhenRoleArrives_d__33), nameof(IntroCutscene._RetryTaskTextWhenRoleArrives_d__33.MoveNext))]
    internal static class IntroCutscene_RetryTaskTextWhenRoleArrives_MoveNext_Patch
    {
        private static void Postfix()
        {
            var intro = IntroCutscene.Instance;
            if (intro == null || intro.Title == null || intro.DescriptionText == null) return;

            var local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null || local.Data.myRole == null) return;

            var d = RoleRegistry.Find(local.Data.myRole) ?? RoleRegistry.FindAssigned(local);
            if (d == null) return;

            intro.Title.text = d.DisplayName;
            intro.Title.color = d.TeamColor;
            intro.DescriptionText.text = d.DescriptionShort;
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.Start))]
    internal static class RoleManager_Start_Patch
    {
        private static void Postfix() { }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    internal static class PlayerControl_FixedUpdate_RoleRegistry_Patch
    {
        private static string _lastLocalRoleTypeName;

        private static void Prefix(PlayerControl __instance)
        {
            if (__instance != PlayerControl.LocalPlayer) return;
            RoleRegistry.ProcessPendingAssignments();
            RoleRegistry.ReapplyIfCustomRole(__instance);

            var role = __instance.Data != null ? __instance.Data.myRole : null;
            string current = role != null ? role.GetIl2CppType().Name : "<null>";
            if (current != _lastLocalRoleTypeName)
            {
                ManuAPIPlugin.Log.LogWarning("Local player myRole changed: " + (_lastLocalRoleTypeName ?? "<none>") + " -> " + current);
                _lastLocalRoleTypeName = current;
            }
        }

        private static void Postfix(PlayerControl __instance)
        {
            if (__instance == PlayerControl.LocalPlayer)
                IntroText.ApplyCurrent();
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__29), nameof(IntroCutscene._CoBegin_d__29.MoveNext))]
    internal static class IntroCutscene_CoBegin_MoveNext_Patch
    {
        private static void Postfix() => IntroText.ApplyCurrent();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.FixedUpdate))]
    internal static class HudManager_FixedUpdate_RoleRegistry_Patch
    {
        private static void Prefix(HudManager __instance)
        {
            RoleRegistry.ProcessPendingAssignments();
            RoleRegistry.UpdateKillButtonVisibility(__instance);
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnAssign))]
    internal static class RoleBehaviour_OnAssign_Patch
    {
        private static void Postfix(RoleBehaviour __instance, PlayerControl player) => RoleRegistry.ApplyOnAssign(__instance, player);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRole))]
    internal static class PlayerControl_SetRole_Patch
    {
        private static bool Prefix(PlayerControl __instance)
        {
            try { return !RoleRegistry.HasCustomRoleAssigned(__instance); }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("PlayerControl.SetRole guard: " + e);
                return true;
            }
        }

        private static void Postfix(PlayerControl __instance)
        {
            try { RoleRegistry.ReapplyIfCustomRole(__instance); }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("PlayerControl.SetRole reapply: " + e); }
        }
    }

    internal static class RoleRegistrationForcer
    {
        public static void Force()
        {
            try
            {
                ManactorAPI.FlushPendingIl2CppTypeRegistrations();
                RoleRegistry.EnsureAllTypesRegistered();
                ManactorAPI.FlushPendingIl2CppTypeRegistrations();
                RoleRegistry.EnsureAllTypesRegistered();
            }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("Flush pending role registrations: " + e); }
        }
    }

    internal static class FreeplayRoleFolderRegistration
    {
        public static void Force(string source)
        {
            RoleRegistrationForcer.Force();

            try
            {
                if (RoleManager.InstanceExists && RoleManager.Instance.allRoles != null)
                    ManuAPIPlugin.Log.LogDebug($"Freeplay role folder registration forced from {source}; roles={RoleManager.Instance.allRoles.Count}.");
            }
            catch (Exception e) { ManuAPIPlugin.Log.LogError($"Freeplay role folder registration ({source}): " + e); }
        }

        public static void AddMissingRoleButtons(TaskAdderGame game)
        {
            Force("TaskAdderGame.OpenRoleFolder.Postfix");

            try
            {
                if (game == null || game.RoleButton == null || game.TaskParent == null || game.ActiveItems == null) return;

                var visibleRoleNames = new HashSet<string>();
                for (int i = 0; i < game.ActiveItems.Count; i++)
                {
                    var item = game.ActiveItems[i];
                    if (item == null) continue;

                    var existingButton = item.GetComponent<TaskAddButton>();
                    if (existingButton != null && existingButton.IsRole && !string.IsNullOrEmpty(existingButton.RoleName))
                        visibleRoleNames.Add(existingButton.RoleName);
                }

                int added = 0;
                foreach (var role in RoleRegistry.RegisteredCustomRoles())
                {
                    if (role == null || string.IsNullOrEmpty(role.AssignedRoleName)) continue;
                    if (!visibleRoleNames.Add(role.AssignedRoleName)) continue;

                    var clone = UnityEngine.Object.Instantiate(game.RoleButton.gameObject, game.TaskParent);
                    clone.name = "ManuAPI_" + role.RoleTypeName + "_FreeplayRoleButton";

                    var setRoleButton = clone.GetComponent<SetRoleButton>();
                    if (setRoleButton != null) setRoleButton.enabled = false;

                    var taskButton = clone.GetComponent<TaskAddButton>();
                    if (taskButton == null)
                    {
                        UnityEngine.Object.Destroy(clone);
                        continue;
                    }

                    taskButton.IsRole = true;
                    taskButton.RoleName = role.AssignedRoleName;
                    taskButton.MyTask = null;
                    if (taskButton.Text != null) taskButton.Text.text = role.DisplayName;

                    PositionAppendedButton(game, clone.transform);
                    game.ActiveItems.Add(clone.transform);
                    clone.SetActive(true);
                    added++;
                }

                if (added > 0)
                {
                    game.ApplyClickMask();
                    ManuAPIPlugin.Log.LogInfo($"Added {added} ManuAPI role button(s) to the Freeplay role folder.");
                }
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("Add ManuAPI role buttons to Freeplay role folder: " + e);
            }
        }

        private static void PositionAppendedButton(TaskAdderGame game, Transform target)
        {
            var rolePositions = new List<Vector3>();
            for (int i = 0; i < game.ActiveItems.Count; i++)
            {
                var item = game.ActiveItems[i];
                if (item == null) continue;

                var button = item.GetComponent<TaskAddButton>();
                if (button != null && button.IsRole)
                    rolePositions.Add(item.localPosition);
            }

            if (rolePositions.Count <= 0)
            {
                target.localPosition = game.RoleButton.transform.localPosition;
                return;
            }

            if (rolePositions.Count == 1)
            {
                float spacing = Math.Abs(game.lineHeight);
                if (spacing < 0.05f) spacing = 0.45f;

                var fallbackNext = rolePositions[0];
                fallbackNext.y -= spacing;
                target.localPosition = fallbackNext;
                return;
            }

            var delta = rolePositions[rolePositions.Count - 1] - rolePositions[rolePositions.Count - 2];
            if (delta.sqrMagnitude < 0.0025f)
                delta = new Vector3(0f, -Math.Max(Math.Abs(game.lineHeight), 0.45f), 0f);

            var projectedNext = rolePositions[rolePositions.Count - 1] + delta;
            target.localPosition = projectedNext;
        }
    }

    [HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.Begin))]
    internal static class TaskAdderGame_Begin_RoleRegistry_Patch
    {
        private static void Prefix() => FreeplayRoleFolderRegistration.Force("TaskAdderGame.Begin");
    }

    [HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.OpenRoleFolder))]
    internal static class TaskAdderGame_OpenRoleFolder_RoleRegistry_Patch
    {
        private static void Prefix() => FreeplayRoleFolderRegistration.Force("TaskAdderGame.OpenRoleFolder");

        private static void Postfix(TaskAdderGame __instance) => FreeplayRoleFolderRegistration.AddMissingRoleButtons(__instance);
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRolesForTeam))]
    internal static class RoleManager_AssignRolesForTeam_Patch
    {
        private static void Prefix()
        {
        }

        private static void Postfix(RoleManager __instance, RoleTeamTypes type, int max)
        {
            var client = AmongUsClient.Instance;
            if (client == null || !client.AmHost) return;

            try { RoleRegistry.AssignForTeam(__instance, type); }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("RoleRegistry.AssignForTeam: " + e); }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    internal static class ExileController_Begin_Patch
    {
        private static void Prefix(ExileController __instance, GameData.PlayerInfo exiled, bool tie)
        {
            if (__instance == null || exiled == null) return;
            try
            {
                string text = RoleRegistry.ResolveEjectionText(exiled);
                if (text == null) return;

                if (__instance.Text != null) __instance.Text.Text = text;
                __instance.completeString = text;
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("ExileController.Begin text patch: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(ExileController._Animate_d__17), nameof(ExileController._Animate_d__17.MoveNext))]
    internal static class ExileController_Animate_MoveNext_Patch
    {
        private static void Postfix(ExileController._Animate_d__17 __instance)
        {
            var ctrl = __instance == null ? null : __instance.__4__this;
            if (ctrl == null || ctrl.exiled == null) return;
            try
            {
                string text = RoleRegistry.ResolveEjectionText(ctrl.exiled);
                if (text == null) return;

                ctrl.completeString = text;
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("ExileController.Animate text patch: " + e);
            }
        }
    }
}
