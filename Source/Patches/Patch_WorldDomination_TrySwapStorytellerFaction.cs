using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // World Domination's swap for blocked storyteller raids runs after the raid patch here, so it can't choose outside the range
    [HarmonyPatch]
    public static class Patch_WorldDomination_TrySwapStorytellerFaction
    {
        private static readonly MethodBase? Target = WorldDominationCompat.StorytellerGate == null ? null
            : AccessTools.Method(WorldDominationCompat.StorytellerGate, "TrySwapToAllowedStorytellerFaction");

        public static bool Prepare() => Target != null;

        public static MethodBase? TargetMethod() => Target;

        public static void Postfix(ref Faction swap, ref bool __result)
        {
            if (!__result || swap == null || !WorldDominationCompat.Active || TechPolicy.Eligible(swap)) return;
            swap = null!;
            __result = false;
        }
    }
}
