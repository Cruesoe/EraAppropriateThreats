using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // Rerolling keeps the event's own exclusions
    [HarmonyPatch]
    public static class Patch_HuntingParty_TryFindFaction
    {
        private const int Rerolls = 30;

        private static bool rerolling;

        public static bool Prepare() => HuntingPartyCompat.WorkerType != null;

        public static MethodBase TargetMethod() => AccessTools.DeclaredMethod(HuntingPartyCompat.WorkerType, "TryFindFaction");

        public static void Postfix(object __instance, MethodBase __originalMethod, ref Faction __result)
        {
            if (rerolling || __result == null || !HuntingPartyCompat.Active || TechPolicy.Eligible(__result)) return;
            rerolling = true;
            try
            {
                for (int i = 0; i < Rerolls; i++)
                {
                    if (__originalMethod.Invoke(__instance, null) is Faction faction && TechPolicy.Eligible(faction))
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
