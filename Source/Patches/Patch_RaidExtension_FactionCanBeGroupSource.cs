using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // [SR]Raid Extension's hostile groups replace the game's check instead of calling it, so they need their own patch
    [HarmonyPatch]
    public static class Patch_RaidExtension_FactionCanBeGroupSource
    {
        private static readonly MethodBase?[] Targets = new[] { "IncidentWorkerHostileTraveler", "IncidentWorkerHostileTraderCaravanPassing" }
            .Select(name => AccessTools.TypeByName("SR.ModRimWorld.RaidExtension." + name))
            .Where(type => type != null)
            .Select(type => (MethodBase?)AccessTools.DeclaredMethod(type, "FactionCanBeGroupSource"))
            .ToArray();

        public static bool Prepare() => Targets.Any(target => target != null);

        public static IEnumerable<MethodBase> TargetMethods() => Targets.Where(target => target != null).Cast<MethodBase>();

        public static void Postfix(Faction f, IncidentParms parms, ref bool __result)
        {
            if (!__result || !ThreatsMod.Settings.filterRaids || parms.faction != null || parms.forced || !TechPolicy.InGame) return;
            __result = TechPolicy.Eligible(f);
        }
    }
}
