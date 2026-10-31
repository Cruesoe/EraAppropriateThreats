using HarmonyLib;
using RimWorld;
using Verse;

namespace EraAppropriateThreats
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
    public static class Patch_IncidentWorker_RaidEnemy_TryResolveRaidFaction
    {
        public static void Prefix(IncidentParms parms, out (bool filter, Faction? questFaction) __state)
        {
            __state = (false, null);
            ThreatSettings settings = ThreatsMod.Settings;
            if (!settings.filterRaids || !TechPolicy.InGame) return;
            if (parms.faction == null)
            {
                __state.filter = true;
            }
            // Quest raids name their faction; clearing it lets the game pick one, which the postfix then checks
            else if (settings.replaceQuestRaiders && parms.pawnKind == null && (parms.quest != null || !string.IsNullOrEmpty(parms.questTag))
                && !TechPolicy.Eligible(parms.faction))
            {
                __state = (true, parms.faction);
                parms.faction = null;
            }
        }

        // Runs before World Domination's own gate on this method, which has the last word on storyteller raids
        [HarmonyPriority(Priority.High)]
        public static void Postfix(IncidentWorker_RaidEnemy __instance, IncidentParms parms, (bool filter, Faction? questFaction) __state, ref bool __result)
        {
            if (!__state.filter || __result && TechPolicy.Eligible(parms.faction)) return;
            if (__result && TechPolicy.RuleFor(parms.faction.def) != FactionRule.Never && Rand.Chance(ThreatsMod.Settings.outOfRangeChance)) return;
            Faction? picked = parms.faction;
            __result = TechPolicy.TryChooseRaidFaction(__instance, parms, out Faction? faction);
            parms.faction = faction;
            if (__result) return;
            // Quests and forced raids can be waiting on this raid, so they keep their own faction or the game's pick if nothing else can come
            Faction? fallback = __state.questFaction ?? (parms.forced ? picked : null);
            if (fallback == null) return;
            parms.faction = fallback;
            __result = true;
        }
    }
}
