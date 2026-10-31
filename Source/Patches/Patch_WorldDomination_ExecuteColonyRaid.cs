using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // Every army reaching a player colony raids through here, including those launched by routes that skip FinalizeRaid
    [HarmonyPatch]
    public static class Patch_WorldDomination_ExecuteColonyRaid
    {
        private static readonly MethodBase? Target = WorldDominationCompat.ColonyRaids == null ? null
            : AccessTools.Method(WorldDominationCompat.ColonyRaids, "ExecuteWdColonyRaidIncidentFromFaction");

        public static bool Prepare() => Target != null;

        public static MethodBase? TargetMethod() => Target;

        public static bool Prefix(Faction attackerFaction, ref bool __result)
        {
            if (attackerFaction == null || !WorldDominationCompat.Active || TechPolicy.Eligible(attackerFaction)) return true;
            __result = false;
            return false;
        }
    }
}
