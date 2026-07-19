using HarmonyLib;
using InnerNet;

namespace ClassicUs.ManuAPI
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    internal static class PlayerControl_MurderPlayer_GameEvents_Patch
    {
        private static bool Prefix(PlayerControl __instance, PlayerControl target) =>
            GameEvents.RaiseBeforeMurder(__instance, target);

        private static void Postfix(PlayerControl __instance, PlayerControl target) =>
            GameEvents.RaiseAfterMurder(__instance, target);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    internal static class PlayerControl_CmdReportDeadBody_GameEvents_Patch
    {
        private static bool Prefix(PlayerControl __instance, GameData.PlayerInfo target) =>
            GameEvents.RaiseBeforeReport(__instance, target);

        private static void Postfix(PlayerControl __instance, GameData.PlayerInfo target) =>
            GameEvents.RaiseAfterReport(__instance, target);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    internal static class MeetingHud_Start_GameEvents_Patch
    {
        private static void Postfix(MeetingHud __instance) => GameEvents.RaiseMeeting(__instance);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    internal static class MeetingHud_Close_GameEvents_Patch
    {
        private static void Prefix(MeetingHud __instance) => GameEvents.RaiseAfterMeeting(__instance);
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcEnterVent))]
    internal static class PlayerPhysics_RpcEnterVent_GameEvents_Patch
    {
        private static void Postfix(PlayerPhysics __instance, int id) => GameEvents.RaiseVent(__instance, id, true);
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ExitVent))]
    internal static class PlayerPhysics_ExitVent_GameEvents_Patch
    {
        private static void Postfix(PlayerPhysics __instance, int id) => GameEvents.RaiseVent(__instance, id, false);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    internal static class PlayerControl_CompleteTask_GameEvents_Patch
    {
        private static void Postfix(PlayerControl __instance, uint idx) => GameEvents.RaiseTaskCompleted(__instance, idx);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    internal static class PlayerControl_Exiled_GameEvents_Patch
    {
        private static void Postfix(PlayerControl __instance) => GameEvents.RaiseExiled(__instance);
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    internal static class HudManager_Start_GameEvents_Patch
    {
        private static void Postfix() => GameEvents.RaiseGameStarted();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    internal static class AmongUsClient_OnGameEnd_GameEvents_Patch
    {
        private static void Prefix(GameOverReason gameOverReason) => GameEvents.RaiseGameEnded(gameOverReason);
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    internal static class AmongUsClient_OnPlayerJoined_GameEvents_Patch
    {
        private static void Postfix(ClientData data) => GameEvents.RaisePlayerJoined(data);
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    internal static class AmongUsClient_OnPlayerLeft_GameEvents_Patch
    {
        private static void Prefix(ClientData data, DisconnectReasons reason) => GameEvents.RaisePlayerLeft(data, reason);
    }
}
