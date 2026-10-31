using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // The IncidentParms overload forwards to this one
    [HarmonyPatch(typeof(StorytellerComp), "UsableIncidentsInCategory", typeof(IncidentCategoryDef), typeof(Func<IncidentDef, IncidentParms>))]
    public static class Patch_StorytellerComp_UsableIncidentsInCategory
    {
        public static void Postfix(ref IEnumerable<IncidentDef> __result)
        {
            if (!ThreatsMod.Settings.filterIncidents || !TechPolicy.InGame) return;
            __result = __result.Where(incident =>
            {
                FactionDef? source = TechPolicy.IncidentSource(incident);
                return source == null || TechPolicy.Eligible(source);
            });
        }
    }
}
