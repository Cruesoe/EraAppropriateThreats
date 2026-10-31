using System;
using HarmonyLib;

namespace EraAppropriateThreats
{
    // World Domination 2.0 sends raids as armies crossing the world map, and keeps its own gate on storyteller raids
    public static class WorldDominationCompat
    {
        public static readonly Type? RaidActions = AccessTools.TypeByName("TSA_WorldDomination.WorldActions_Raid");
        public static readonly Type? ColonyRaids = AccessTools.TypeByName("TSA_WorldDomination.Raid_OnPlayerColony");
        public static readonly Type? StorytellerGate = AccessTools.TypeByName("TSA_WorldDomination.Patch_RaidEnemy_AdjustPoints");
        public static readonly Type? Evaluation = AccessTools.TypeByName("TSA_WorldDomination.PendingRaidEvaluation");
        public static readonly Type? Candidate = AccessTools.TypeByName("TSA_WorldDomination.RaidTargetCandidate");

        public static bool Active => ThreatsMod.Settings.filterRaids && TechPolicy.InGame;
    }
}
