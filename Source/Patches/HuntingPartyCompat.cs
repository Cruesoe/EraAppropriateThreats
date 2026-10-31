using System;
using HarmonyLib;

namespace EraAppropriateThreats
{
    // Vanilla Events Expanded's hunting parties choose from non-hostile factions, so they follow the traders and visitors setting
    public static class HuntingPartyCompat
    {
        public static readonly Type? WorkerType = AccessTools.TypeByName("VEE.HuntingParty");

        public static bool Active => ThreatsMod.Settings.filterVisitors && TechPolicy.InGame;
    }
}
