using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ClassicUs.Manactor;
using HarmonyLib;

namespace ClassicUs.ManuAPI
{
    [BepInPlugin(Guid, "Classic Us ManuAPI", Version)]
    [BepInDependency(ManactorPlugin.Guid)]
    public class ManuAPIPlugin : BasePlugin
    {
        public const string Guid = "classicus.manuapi";
        public const string Version = "1.3.17";

        public static ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;
            var harmony = new Harmony(Guid);
            harmony.PatchAll();
            Il2CppInteropCompat.Apply(harmony);
            RoleRegistry.RegisterNetworkHandlers();
            Log.LogInfo("ClassicUs.ManuAPI loaded.");
        }
    }
}
