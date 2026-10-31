using System;
using HarmonyLib;
using RimWorld;

namespace EraAppropriateThreats
{
    // Caravan ambushes and demands choose their faction here; with no eligible faction they don't fire
    [HarmonyPatch(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup))]
    public static class Patch_PawnGroupMakerUtility_TryGetRandomFactionForCombatPawnGroup
    {
        public static void Prefix(ref Predicate<Faction>? validator)
        {
            if (!ThreatsMod.Settings.filterRaids || !TechPolicy.InGame) return;
            Predicate<Faction>? original = validator;
            validator = f => TechPolicy.Eligible(f) && (original == null || original(f));
        }
    }
}
