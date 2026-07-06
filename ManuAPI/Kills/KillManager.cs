using ClassicUs.Manactor;

namespace ClassicUs.ManuAPI
{
    public sealed class KillRequest
    {
        public bool TeleportKiller = true;
        public bool CreateDeadBody = true;
        public bool PlayKillSound = true;
        public bool ShowKillAnimation = true;
        public MurderResultFlags ResultFlags = MurderResultFlags.Succeeded;
    }

    public static class KillManager
    {
        public static void Kill(PlayerControl killer, PlayerControl target, KillRequest request = null)
        {
            request ??= new KillRequest();

            ManactorAPI.KillPlayer(killer, target, new CustomKillOptions
            {
                TeleportKiller = request.TeleportKiller,
                CreateDeadBody = request.CreateDeadBody,
                PlayKillSound = request.PlayKillSound,
                ShowKillAnimation = request.ShowKillAnimation,
                ResultFlags = request.ResultFlags,
            });
        }
    }
}
