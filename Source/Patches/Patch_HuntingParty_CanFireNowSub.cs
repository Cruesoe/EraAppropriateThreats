using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EraAppropriateThreats
{
    // The event assumes it gets a faction, so it can't fire unless one in range exists
    [HarmonyPatch]
    public static class Patch_HuntingParty_CanFireNowSub
    {
        public static bool Prepare() => HuntingPartyCompat.WorkerType != null;

        public static MethodBase TargetMethod() => AccessTools.DeclaredMethod(HuntingPartyCompat.WorkerType, "CanFireNowSub");

        public static void Postfix(ref bool __result)
        {
            if (!__result || !HuntingPartyCompat.Active) return;
            __result = Find.FactionManager.AllFactions.Any(f => !f.IsPlayer && !f.Hidden && !f.defeated && f.def.techLevel >= TechLevel.Neolithic
                && !f.HostileTo(Faction.OfPlayer) && TechPolicy.Eligible(f));
        }
    }
}
