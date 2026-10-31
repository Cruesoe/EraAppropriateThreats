using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EraAppropriateThreats
{
    // Vanilla Events Expanded's hunting parties choose from non-hostile factions, so they follow the traders and visitors setting
    public static class HuntingPartyCompat
    {
        public static readonly Type? WorkerType = AccessTools.TypeByName("VEE.HuntingParty");

        public static bool Active => ThreatsMod.Settings.filterVisitors && TechPolicy.InGame;
    }

    // The event assumes it gets a faction, so it can't fire unless one in range exists
    [HarmonyPatch]
    public static class Patch_HuntingParty_CanFireNowSub
    {
        public static bool Prepare() => HuntingPartyCompat.WorkerType != null;

        public static MethodBase TargetMethod() => AccessTools.DeclaredMethod(HuntingPartyCompat.WorkerType, "CanFireNowSub");

        public static void Postfix(ref bool __result)
        {
            if (!__result || !HuntingPartyCompat.Active) return;
            __result = Find.FactionManager.AllFactions.Any(f => !f.IsPlayer && !f.Hidden && !f.defeated && !f.HostileTo(Faction.OfPlayer) && TechPolicy.Eligible(f));
        }
    }

    // Rerolling keeps the event's own exclusions
    [HarmonyPatch]
    public static class Patch_HuntingParty_TryFindFaction
    {
        private const int Rerolls = 30;

        private static bool rerolling;

        public static bool Prepare() => HuntingPartyCompat.WorkerType != null;

        public static MethodBase TargetMethod() => AccessTools.DeclaredMethod(HuntingPartyCompat.WorkerType, "TryFindFaction");

        public static void Postfix(object __instance, MethodBase __originalMethod, ref Faction __result)
        {
            if (rerolling || __result == null || !HuntingPartyCompat.Active || TechPolicy.Eligible(__result)) return;
            rerolling = true;
            try
            {
                for (int i = 0; i < Rerolls; i++)
                {
                    if (__originalMethod.Invoke(__instance, null) is Faction faction && TechPolicy.Eligible(faction))
                    {
                        __result = faction;
                        return;
                    }
                }
            }
            finally
            {
                rerolling = false;
            }
        }
    }
}
