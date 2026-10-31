using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // World Domination's swap for blocked storyteller raids runs after the raid patch here, so it can't choose outside the range; rerolling keeps its own exclusions
    [HarmonyPatch]
    public static class Patch_WorldDomination_TrySwapStorytellerFaction
    {
        private const int Rerolls = 30;

        private static readonly MethodBase? Target = WorldDominationCompat.StorytellerGate == null ? null
            : AccessTools.Method(WorldDominationCompat.StorytellerGate, "TrySwapToAllowedStorytellerFaction");
        private static bool rerolling;

        public static bool Prepare() => Target != null;

        public static MethodBase? TargetMethod() => Target;

        public static void Postfix(IncidentParms parms, ref Faction swap, ref bool __result)
        {
            if (rerolling || !__result || swap == null || !WorldDominationCompat.Active || TechPolicy.Eligible(swap)) return;
            rerolling = true;
            try
            {
                for (int i = 0; i < Rerolls; i++)
                {
                    object?[] args = { parms, null };
                    if (Target!.Invoke(null, args) is true && args[1] is Faction faction && TechPolicy.Eligible(faction))
                    {
                        swap = faction;
                        return;
                    }
                }
            }
            finally
            {
                rerolling = false;
            }
            swap = null!;
            __result = false;
        }
    }
}
