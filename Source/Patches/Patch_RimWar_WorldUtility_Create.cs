using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace EraAppropriateThreats
{
    // Rim War sends raids as units crossing the world map; out-of-range hostile settlements don't send them at the player
    [HarmonyPatch]
    public static class Patch_RimWar_WorldUtility_Create
    {
        private static readonly Type? UtilityType = AccessTools.TypeByName("RimWar.Planet.WorldUtility");

        public static bool Prepare() => UtilityType != null;

        public static IEnumerable<MethodBase> TargetMethods()
            => new[] { "CreateWarband", "CreateLaunchedWarband", "CreateScout" }
                .Select(name => AccessTools.Method(UtilityType, name))
                .Where(method => method != null)
                .Cast<MethodBase>();

        public static bool Prefix(Settlement parentSettlement, WorldObject destination)
        {
            if (!ThreatsMod.Settings.filterRaids || !TechPolicy.InGame || destination?.Faction != Faction.OfPlayer) return true;
            Faction? sender = parentSettlement?.Faction;
            return sender == null || !sender.HostileTo(Faction.OfPlayer) || TechPolicy.Eligible(sender);
        }
    }
}
