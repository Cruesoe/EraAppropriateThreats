using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // Every storyteller route checks this before firing; forced events, such as most quest threats, are left alone
    [HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.CanFireNow), typeof(IncidentParms))]
    public static class Patch_IncidentWorker_CanFireNow
    {
        public static void Postfix(IncidentWorker __instance, IncidentParms parms, ref bool __result)
        {
            if (!__result || parms.forced || !ThreatsMod.Settings.filterIncidents || !TechPolicy.InGame) return;
            __result = TechPolicy.IncidentAllowed(__instance.def);
        }
    }
}
