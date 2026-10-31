using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // Traders and visitors only; raids choose their faction in Patch_IncidentWorker_RaidEnemy_TryResolveRaidFaction
    [HarmonyPatch(typeof(IncidentWorker_PawnsArrive), nameof(IncidentWorker_PawnsArrive.FactionCanBeGroupSource))]
    public static class Patch_IncidentWorker_PawnsArrive_FactionCanBeGroupSource
    {
        public static void Postfix(IncidentWorker_PawnsArrive __instance, Faction f, IncidentParms parms, ref bool __result)
        {
            if (!__result || !ThreatsMod.Settings.filterVisitors || !(__instance is IncidentWorker_NeutralGroup)) return;
            if (parms.faction != null || parms.forced || !TechPolicy.InGame) return;
            __result = TechPolicy.Eligible(f);
        }
    }
}
