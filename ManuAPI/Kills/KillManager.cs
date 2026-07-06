using ClassicUs.Manactor;

namespace ClassicUs.ManuAPI
{
    public sealed class KillRequest
    {
        public bool TeleportKiller = true;
        public MurderResultFlags ResultFlags = MurderResultFlags.Succeeded;
    }

    public static class KillManager
    {
        public static void Kill(PlayerControl killer, PlayerControl target, KillRequest request = null)
        {
            request ??= new KillRequest();

            ManactorAPI.KillPlayer(killer, target, new KillOptions
            {
                WillTeleportMurder = request.TeleportKiller,
                ResultFlags = request.ResultFlags,
            });
        }
    }
}
