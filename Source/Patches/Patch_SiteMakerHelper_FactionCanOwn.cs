using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace EraAppropriateThreats
{
    // Quest sites choose their owner here, including VFE Medieval 2's siege camps; a quest with no eligible owner isn't offered
    [HarmonyPatch]
    public static class Patch_SiteMakerHelper_FactionCanOwn
    {
        private static readonly MethodBase? Target = AccessTools.Method(typeof(SiteMakerHelper), "FactionCanOwn",
            new[] { typeof(SitePartDef), typeof(Faction), typeof(bool), typeof(Predicate<Faction>) });

        public static bool Prepare() => Target != null;

        public static MethodBase? TargetMethod() => Target;

        public static void Postfix(Faction faction, ref bool __result)
        {
            if (!__result || faction == null || !ThreatsMod.Settings.filterQuestSites || !TechPolicy.InGame) return;
            if (faction.IsPlayer || !faction.HostileTo(Faction.OfPlayer)) return;
            __result = TechPolicy.Eligible(faction);
        }
    }
}
