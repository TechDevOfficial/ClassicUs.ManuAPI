using HarmonyLib;

namespace ClassicUs.ManuAPI
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    internal static class HudManager_Start_GameMode_Patch
    {
        private static void Postfix() => GameModeRegistry.NotifyGameStarted();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.FixedUpdate))]
    internal static class HudManager_FixedUpdate_GameMode_Patch
    {
        private static void Postfix() => GameModeRegistry.Update();
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    internal static class MeetingHud_Start_GameMode_Patch
    {
        private static void Postfix(MeetingHud __instance) => GameModeRegistry.MeetingStarted(__instance);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    internal static class MeetingHud_Close_GameMode_Patch
    {
        private static void Prefix(MeetingHud __instance) => GameModeRegistry.MeetingEnded(__instance);
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    internal static class ShipStatus_CheckEndCriteria_GameMode_Patch
    {
        private static bool Prefix() => !GameModeRegistry.TryHandleEndCriteria();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    internal static class AmongUsClient_OnGameEnd_GameMode_Patch
    {
        private static void Prefix(GameOverReason gameOverReason) => GameModeRegistry.NotifyGameEnded(gameOverReason);
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    internal static class AmongUsClient_ExitGame_GameMode_Patch
    {
        private static void Prefix() => GameModeRegistry.ResetMatchState();
    }
}
