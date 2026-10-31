using HarmonyLib;
using RimWorld;
using Verse;

namespace EraAppropriateThreats
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
    public static class Patch_IncidentWorker_RaidEnemy_TryResolveRaidFaction
    {
        public static void Prefix(IncidentParms parms, out bool __state)
        {
            __state = false;
            ThreatSettings settings = ThreatsMod.Settings;
            if (!settings.filterRaids || !TechPolicy.InGame) return;
            if (parms.faction == null)
            {
                __state = !parms.forced;
            }
            // Quest raids name their faction; clearing it lets the game pick one, which the postfix then checks
            else if (settings.replaceQuestRaiders && parms.pawnKind == null && !string.IsNullOrEmpty(parms.questTag)
                && !TechPolicy.Eligible(parms.faction))
            {
                parms.faction = null;
                __state = true;
            }
        }

        public static void Postfix(IncidentWorker_RaidEnemy __instance, IncidentParms parms, bool __state, ref bool __result)
        {
            if (!__state || __result && TechPolicy.Eligible(parms.faction)) return;
            if (__result && TechPolicy.RuleFor(parms.faction.def) != FactionRule.Never && Rand.Chance(ThreatsMod.Settings.outOfRangeChance)) return;
            __result = TechPolicy.TryChooseRaidFaction(__instance, parms, out Faction? faction);
            parms.faction = faction;
        }
    }
}
