using System.Collections;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace EraAppropriateThreats
{
    // Takes the player off an out-of-range settlement's target list, so it picks another target or grows instead
    [HarmonyPatch]
    public static class Patch_WorldDomination_FinalizeRaid
    {
        private static readonly MethodBase? Target = WorldDominationCompat.RaidActions == null ? null
            : AccessTools.Method(WorldDominationCompat.RaidActions, "FinalizeRaid");
        private static readonly FieldInfo? AttackerField = WorldDominationCompat.Evaluation == null ? null
            : AccessTools.Field(WorldDominationCompat.Evaluation, "attacker");
        private static readonly FieldInfo? ViableField = WorldDominationCompat.Evaluation == null ? null
            : AccessTools.Field(WorldDominationCompat.Evaluation, "viable");
        private static readonly FieldInfo? CandidateTargetField = WorldDominationCompat.Candidate == null ? null
            : AccessTools.Field(WorldDominationCompat.Candidate, "Target");

        public static bool Prepare() => Target != null && AttackerField != null && ViableField != null && CandidateTargetField != null;

        public static MethodBase? TargetMethod() => Target;

        public static void Prefix(object eval)
        {
            if (!WorldDominationCompat.Active || !(AttackerField!.GetValue(eval) is Settlement attacker)) return;
            if (attacker.Faction == null || TechPolicy.Eligible(attacker.Faction) || !(ViableField!.GetValue(eval) is IList viable)) return;
            for (int i = viable.Count - 1; i >= 0; i--)
            {
                if (CandidateTargetField!.GetValue(viable[i]) is WorldObject target && target.Faction == Faction.OfPlayer) viable.RemoveAt(i);
            }
        }
    }
}
