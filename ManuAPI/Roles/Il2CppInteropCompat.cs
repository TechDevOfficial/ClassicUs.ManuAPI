using System;
using System.Reflection;
using HarmonyLib;

namespace ClassicUs.ManuAPI
{
    internal static class Il2CppInteropCompat
    {
        public static void Apply(Harmony harmony)
        {
            var method = AccessTools.Method(
                "Il2CppInterop.Runtime.Injection.ClassInjector:RewriteType",
                new[] { typeof(Type) });

            if (method == null)
            {
                ManuAPIPlugin.Log.LogWarning("Il2CppInteropCompat: RewriteType method not found, skipping patch");
                return;
            }

            var prefix = new HarmonyMethod(typeof(Il2CppInteropCompat).GetMethod(nameof(Prefix), BindingFlags.NonPublic | BindingFlags.Static));
            harmony.Patch(method, prefix);
        }

        private static bool Prefix(Type type, ref Type __result)
        {
            if (type != null && type.Namespace == null && type.FullName != null && type.FullName.StartsWith("System"))
            {
                __result = type;
                return false;
            }

            return true;
        }
    }
}
