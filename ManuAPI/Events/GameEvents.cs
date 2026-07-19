using System;
using HarmonyLib;
using InnerNet;

namespace ClassicUs.ManuAPI
{
    /// <summary>
    /// Central event surface for gameplay actions. Subscribe from your plugin Load method and
    /// unsubscribe when your plugin is unloaded. Event handlers are isolated so one mod cannot
    /// break the game loop for another mod.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<MurderEventArgs> BeforeMurder;
        public static event Action<MurderEventArgs> AfterMurder;
        public static event Action<ReportEventArgs> BeforeReport;
        public static event Action<ReportEventArgs> AfterReport;
        public static event Action<MeetingEventArgs> AtMeeting;
        public static event Action<MeetingEventArgs> AfterMeeting;
        public static event Action<VentEventArgs> PlayerEnteredVent;
        public static event Action<VentEventArgs> PlayerExitedVent;
        public static event Action<TaskCompletedEventArgs> TaskCompleted;
        public static event Action<PlayerEventArgs> PlayerExiled;
        public static event Action<GameStartedEventArgs> GameStarted;
        public static event Action<GameEndedEventArgs> GameEnded;
        public static event Action<PlayerConnectionEventArgs> PlayerJoined;
        public static event Action<PlayerConnectionEventArgs> PlayerLeft;

        private static bool _gameStarted;

        internal static bool RaiseBeforeMurder(PlayerControl killer, PlayerControl target)
        {
            var args = new MurderEventArgs(killer, target);
            Raise(BeforeMurder, args, nameof(BeforeMurder));
            return !args.Cancelled;
        }

        internal static void RaiseAfterMurder(PlayerControl killer, PlayerControl target) =>
            Raise(AfterMurder, new MurderEventArgs(killer, target), nameof(AfterMurder));

        internal static bool RaiseBeforeReport(PlayerControl reporter, GameData.PlayerInfo body)
        {
            var args = new ReportEventArgs(reporter, body);
            Raise(BeforeReport, args, nameof(BeforeReport));
            return !args.Cancelled;
        }

        internal static void RaiseAfterReport(PlayerControl reporter, GameData.PlayerInfo body) =>
            Raise(AfterReport, new ReportEventArgs(reporter, body), nameof(AfterReport));

        internal static void RaiseMeeting(MeetingHud meeting) =>
            Raise(AtMeeting, new MeetingEventArgs(meeting), nameof(AtMeeting));

        internal static void RaiseAfterMeeting(MeetingHud meeting) =>
            Raise(AfterMeeting, new MeetingEventArgs(meeting), nameof(AfterMeeting));

        internal static void RaiseVent(PlayerPhysics physics, int ventId, bool entering)
        {
            var player = physics != null ? physics.myPlayer : null;
            var args = new VentEventArgs(player, ventId);
            Raise(entering ? PlayerEnteredVent : PlayerExitedVent, args,
                entering ? nameof(PlayerEnteredVent) : nameof(PlayerExitedVent));
        }

        internal static void RaiseTaskCompleted(PlayerControl player, uint taskIndex) =>
            Raise(TaskCompleted, new TaskCompletedEventArgs(player, taskIndex), nameof(TaskCompleted));

        internal static void RaiseExiled(PlayerControl player) =>
            Raise(PlayerExiled, new PlayerEventArgs(player), nameof(PlayerExiled));

        internal static void RaiseGameStarted()
        {
            if (_gameStarted) return;
            _gameStarted = true;
            Raise(GameStarted, new GameStartedEventArgs(), nameof(GameStarted));
        }

        internal static void RaiseGameEnded(GameOverReason reason)
        {
            if (!_gameStarted) return;
            _gameStarted = false;
            Raise(GameEnded, new GameEndedEventArgs(reason), nameof(GameEnded));
        }

        internal static void RaisePlayerJoined(ClientData client) =>
            Raise(PlayerJoined, new PlayerConnectionEventArgs(client, null), nameof(PlayerJoined));

        internal static void RaisePlayerLeft(ClientData client, DisconnectReasons reason) =>
            Raise(PlayerLeft, new PlayerConnectionEventArgs(client, reason), nameof(PlayerLeft));

        private static void Raise<T>(Action<T> handlers, T args, string eventName)
        {
            if (handlers == null) return;
            foreach (Action<T> handler in handlers.GetInvocationList())
            {
                try { handler(args); }
                catch (Exception e) { ManuAPIPlugin.Log.LogError("GameEvents." + eventName + ": " + e); }
            }
        }
    }

    public abstract class CancellableGameEventArgs : EventArgs
    {
        /// <summary>Set this in a Before event to stop the original vanilla action.</summary>
        public bool Cancelled { get; set; }
    }

    public sealed class MurderEventArgs : CancellableGameEventArgs
    {
        internal MurderEventArgs(PlayerControl killer, PlayerControl target) { Killer = killer; Target = target; }
        public PlayerControl Killer { get; }
        public PlayerControl Target { get; }
    }

    public sealed class ReportEventArgs : CancellableGameEventArgs
    {
        internal ReportEventArgs(PlayerControl reporter, GameData.PlayerInfo body) { Reporter = reporter; Body = body; }
        public PlayerControl Reporter { get; }
        /// <summary>Null when the reporter pressed the emergency button.</summary>
        public GameData.PlayerInfo Body { get; }
        public bool IsEmergencyMeeting => Body == null;
    }

    public sealed class MeetingEventArgs : EventArgs
    {
        internal MeetingEventArgs(MeetingHud meeting) { Meeting = meeting; }
        public MeetingHud Meeting { get; }
    }

    public sealed class VentEventArgs : EventArgs
    {
        internal VentEventArgs(PlayerControl player, int ventId) { Player = player; VentId = ventId; }
        public PlayerControl Player { get; }
        public int VentId { get; }
    }

    public sealed class TaskCompletedEventArgs : EventArgs
    {
        internal TaskCompletedEventArgs(PlayerControl player, uint taskIndex) { Player = player; TaskIndex = taskIndex; }
        public PlayerControl Player { get; }
        public uint TaskIndex { get; }
    }

    public sealed class PlayerEventArgs : EventArgs
    {
        internal PlayerEventArgs(PlayerControl player) { Player = player; }
        public PlayerControl Player { get; }
    }

    public sealed class GameStartedEventArgs : EventArgs { internal GameStartedEventArgs() { } }

    public sealed class GameEndedEventArgs : EventArgs
    {
        internal GameEndedEventArgs(GameOverReason reason) { Reason = reason; }
        public GameOverReason Reason { get; }
    }

    public sealed class PlayerConnectionEventArgs : EventArgs
    {
        internal PlayerConnectionEventArgs(ClientData client, DisconnectReasons? reason) { Client = client; Reason = reason; }
        public ClientData Client { get; }
        public DisconnectReasons? Reason { get; }
    }
}
