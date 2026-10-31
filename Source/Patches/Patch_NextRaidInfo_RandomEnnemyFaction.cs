using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // Winston Waves chooses each wave's faction here; rerolling keeps its own exclusions and weights
    [HarmonyPatch]
    public static class Patch_NextRaidInfo_RandomEnnemyFaction
    {
        private const int Rerolls = 30;

        private static readonly Type? WavesType = AccessTools.TypeByName("VSEWW.NextRaidInfo");
        private static readonly MethodInfo? Target = WavesType == null ? null : AccessTools.Method(WavesType, "RandomEnnemyFaction");
        private static bool rerolling;

        public static bool Prepare() => Target != null;

        public static MethodBase? TargetMethod() => Target;

        public static void Postfix(object __instance, float points, ref Faction __result)
        {
            if (rerolling || __result == null || !ThreatsMod.Settings.filterRaids || !TechPolicy.InGame || TechPolicy.Eligible(__result)) return;
            rerolling = true;
            try
            {
                for (int i = 0; i < Rerolls; i++)
                {
                    // A wave must have a faction, so the original stays if none of these fit
                    if (Target!.Invoke(__instance, new object[] { points }) is Faction faction && TechPolicy.Eligible(faction))
                    {
                        __result = faction;
                        return;
                    }
                }
            }
            finally
            {
                rerolling = false;
            }
        }
    }
}
