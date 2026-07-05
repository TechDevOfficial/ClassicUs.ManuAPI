using System;
using ClassicUs.Manactor;
using HarmonyLib;
using UnityEngine;

namespace ClassicUs.ManuAPI
{
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

            var d = RoleRegistry.Find(local.Data.myRole);
            if (d == null) return;

            __instance.__4__this.Title.text = d.DisplayName;
            __instance.__4__this.Title.color = d.TeamColor;
            __instance.__4__this.DescriptionText.text = d.Description;
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.Start))]
    internal static class RoleManager_Start_Patch
    {
        private static void Postfix() => RoleRegistry.EnsureAllTypesRegistered();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    internal static class PlayerControl_FixedUpdate_RoleRegistry_Patch
    {
        private static void Prefix(PlayerControl __instance)
        {
            if (__instance != PlayerControl.LocalPlayer) return;
            RoleRegistry.EnsureAllTypesRegistered();
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.FixedUpdate))]
    internal static class HudManager_FixedUpdate_RoleRegistry_Patch
    {
        private static void Prefix(HudManager __instance)
        {
            RoleRegistry.EnsureAllTypesRegistered();
            RoleRegistry.UpdateKillButtonVisibility(__instance);
        }
    }

    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnAssign))]
    internal static class RoleBehaviour_OnAssign_Patch
    {
        private static void Postfix(RoleBehaviour __instance, PlayerControl player) => RoleRegistry.ApplyOnAssign(__instance, player);
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRolesForTeam))]
    internal static class RoleManager_AssignRolesForTeam_Patch
    {
        private static void Prefix()
        {
            var client = AmongUsClient.Instance;
            if (client == null || !client.AmHost) return;

            try
            {
                ManactorAPI.FlushPendingIl2CppTypeRegistrations();
                RoleRegistry.EnsureAllTypesRegistered();
            }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("Flush pending role registrations: " + e); }
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
