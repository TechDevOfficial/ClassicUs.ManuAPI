using System;
using System.Collections.Generic;
using ClassicUs.Manactor;

namespace ClassicUs.ManuAPI
{
    /// <summary>
    /// Base class for a custom Classic Us game mode.
    /// Register an instance with <see cref="GameModeRegistry.Register"/> during your plugin Load method.
    /// </summary>
    public abstract class CustomGameMode
    {
        /// <summary>A stable, globally unique identifier (for example: <c>myplugin.hide-and-seek</c>).</summary>
        public abstract string Id { get; }

        public abstract string Name { get; }

        public virtual string Description => string.Empty;

        /// <summary>
        /// When true, ManuAPI prevents the normal Among Us win checks while this mode is active.
        /// Return a reason from <see cref="CheckEndCriteria"/> to finish the game yourself.
        /// </summary>
        public virtual bool OverrideVanillaWinConditions => false;

        public virtual void OnSelected() { }
        public virtual void OnDeselected() { }
        public virtual void OnGameStarted() { }
        public virtual void OnGameEnded(GameOverReason reason) { }
        public virtual void OnHudUpdate() { }
        public virtual void OnMeetingStarted(MeetingHud meeting) { }
        public virtual void OnMeetingEnded(MeetingHud meeting) { }

        /// <summary>
        /// Called by the host while the game is running. Return a value to end the game; return null to keep playing.
        /// This is only evaluated when <see cref="OverrideVanillaWinConditions"/> is enabled.
        /// </summary>
        public virtual GameOverReason? CheckEndCriteria() => null;
    }

    /// <summary>Registers, selects and drives custom game modes.</summary>
    public static class GameModeRegistry
    {
        private const string SelectModeRpc = "classicus.manuapi.SelectGameMode";
        private static readonly Dictionary<string, CustomGameMode> Modes = new(StringComparer.Ordinal);
        private static string _activeId;
        private static string _pendingRemoteId;
        private static bool _gameStarted;

        public static IReadOnlyCollection<CustomGameMode> RegisteredModes => Modes.Values;
        public static CustomGameMode ActiveGameMode => _activeId != null && Modes.TryGetValue(_activeId, out var mode) ? mode : null;
        public static bool IsHost => AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;

        internal static void RegisterNetworkHandlers() => ManactorAPI.RegisterRpcMethods(typeof(GameModeRegistry));

        public static void Register(CustomGameMode mode)
        {
            if (mode == null) throw new ArgumentNullException(nameof(mode));
            if (string.IsNullOrWhiteSpace(mode.Id)) throw new ArgumentException("A game mode must have a non-empty Id.", nameof(mode));
            if (Modes.ContainsKey(mode.Id)) throw new InvalidOperationException("A game mode with id '" + mode.Id + "' is already registered.");

            Modes.Add(mode.Id, mode);
            if (_pendingRemoteId == mode.Id)
            {
                var pendingId = _pendingRemoteId;
                _pendingRemoteId = null;
                ApplySelection(pendingId);
            }
        }

        /// <summary>Selects a registered game mode. Only the lobby host may call this method.</summary>
        public static bool Select(string id)
        {
            if (!IsHost)
            {
                ManuAPIPlugin.Log.LogWarning("Only the host can select a custom game mode.");
                return false;
            }
            if (id != null && !Modes.ContainsKey(id)) return false;

            ApplySelection(id);
            ManactorAPI.SendRpcMethod(SelectModeRpc, id ?? string.Empty);
            return true;
        }

        public static bool Clear() => Select(null);

        /// <summary>Ends the active match for every player. This method is host-only.</summary>
        public static bool EndGame(GameOverReason reason = GameOverReason.Custom, float delay = 0.25f)
        {
            if (!IsHost || ShipStatus.Instance == null) return false;
            ShipStatus.Instance.StartEndGame(reason, delay);
            return true;
        }

        [ManactorRpc(SelectModeRpc)]
        private static void OnSelectModeRpc(byte senderId, string id)
        {
            var host = AmongUsClient.Instance != null ? AmongUsClient.Instance.HostId : -1;
            if (senderId != host) return;
            ApplySelection(string.IsNullOrEmpty(id) ? null : id);
        }

        private static void ApplySelection(string id)
        {
            if (id == _activeId) return;
            var previous = ActiveGameMode;
            _activeId = null;
            previous?.OnDeselected();

            if (id == null) return;
            if (!Modes.TryGetValue(id, out var next))
            {
                _pendingRemoteId = id;
                ManuAPIPlugin.Log.LogWarning("Received an unregistered custom game mode: " + id);
                return;
            }

            _activeId = id;
            _pendingRemoteId = null;
            next.OnSelected();
            ManuAPIPlugin.Log.LogInfo("Selected custom game mode: " + id);
        }

        internal static void NotifyGameStarted()
        {
            if (_gameStarted) return;
            _gameStarted = true;
            Invoke(mode => mode.OnGameStarted(), "OnGameStarted");
        }

        internal static void NotifyGameEnded(GameOverReason reason)
        {
            if (!_gameStarted) return;
            _gameStarted = false;
            Invoke(mode => mode.OnGameEnded(reason), "OnGameEnded");
        }

        internal static void Update()
        {
            Invoke(mode => mode.OnHudUpdate(), "OnHudUpdate");
        }

        internal static void MeetingStarted(MeetingHud meeting) => Invoke(mode => mode.OnMeetingStarted(meeting), "OnMeetingStarted");
        internal static void MeetingEnded(MeetingHud meeting) => Invoke(mode => mode.OnMeetingEnded(meeting), "OnMeetingEnded");

        internal static bool TryHandleEndCriteria()
        {
            var mode = ActiveGameMode;
            if (mode == null || !mode.OverrideVanillaWinConditions || !IsHost) return false;
            try
            {
                var reason = mode.CheckEndCriteria();
                if (reason.HasValue) EndGame(reason.Value);
                return true;
            }
            catch (Exception e)
            {
                ManuAPIPlugin.Log.LogError("Custom game mode CheckEndCriteria: " + e);
                return false;
            }
        }

        internal static void ResetMatchState() => _gameStarted = false;

        private static void Invoke(Action<CustomGameMode> callback, string hook)
        {
            var mode = ActiveGameMode;
            if (mode == null) return;
            try { callback(mode); }
            catch (Exception e) { ManuAPIPlugin.Log.LogError("Custom game mode " + hook + ": " + e); }
        }
    }
}
