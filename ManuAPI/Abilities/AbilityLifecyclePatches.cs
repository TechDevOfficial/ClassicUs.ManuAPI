using System;
using HarmonyLib;

namespace ClassicUs.ManuAPI
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    internal static class HudManager_Start_AbilityReset_Patch
    {
        private static void Prefix() => AbilityLifecycle.SafeReset("HudManager.Start");
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    internal static class AmongUsClient_OnGameEnd_AbilityReset_Patch
    {
        private static void Prefix() => AbilityLifecycle.SafeReset("AmongUsClient.OnGameEnd");
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.NextGame))]
    internal static class EndGameManager_NextGame_AbilityReset_Patch
    {
        private static void Prefix() => AbilityLifecycle.SafeReset("EndGameManager.NextGame");
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Exit))]
    internal static class EndGameManager_Exit_AbilityReset_Patch
    {
        private static void Prefix() => AbilityLifecycle.SafeReset("EndGameManager.Exit");
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    internal static class AmongUsClient_ExitGame_AbilityReset_Patch
    {
        private static void Prefix() => AbilityLifecycle.SafeReset("AmongUsClient.ExitGame");
    }

    internal static class AbilityLifecycle
    {
        internal static void SafeReset(string reason)
        {
            try { CustomAbility.ResetAll(); }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("CustomAbility.ResetAll (" + reason + "): " + e); }

            try { RoleRegistry.InvalidateForNewMatch(); }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("RoleRegistry.InvalidateForNewMatch (" + reason + "): " + e); }
        }
    }
}
