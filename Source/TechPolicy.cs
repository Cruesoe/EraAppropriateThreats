using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EraAppropriateThreats
{
    public static class TechPolicy
    {
        // One in-game hour, so level changes from any source are picked up without a patch per source
        private const int RefreshTicks = 2500;

        private static Game? cachedGame;
        private static int cachedTick;
        private static TechLevel cachedLevel;
        private static Dictionary<TechLevel, List<ResearchProjectDef>>? projectsByLevel;

        private static ThreatSettings Settings => ThreatsMod.Settings;

        public static bool InGame => Current.Game != null && Faction.OfPlayerSilentFail != null;

        public static TechLevel ColonyLevel
        {
            get
            {
                Game game = Current.Game;
                int ticks = Find.TickManager.TicksGame;
                if (game != cachedGame || ticks < cachedTick || ticks - cachedTick >= RefreshTicks)
                {
                    cachedGame = game;
                    cachedTick = ticks;
                    cachedLevel = CalculateLevel();
                }
                return cachedLevel;
            }
        }

        public static void Invalidate() => cachedGame = null;

        // The player's starting level is the floor: research only raises it
        private static TechLevel CalculateLevel()
        {
            TechLevel floor = Faction.OfPlayer.def.techLevel;
            if (Settings.method == TechMethod.ColonyLevel) return floor;
            projectsByLevel ??= DefDatabase<ResearchProjectDef>.AllDefsListForReading
                .Where(p => p.knowledgeCategory == null)
                .GroupBy(p => p.techLevel)
                .ToDictionary(g => g.Key, g => g.ToList());
            for (TechLevel level = TechLevel.Archotech; level > floor; level--)
            {
                if (!projectsByLevel.TryGetValue(level, out List<ResearchProjectDef>? projects)) continue;
                int finished = projects.Count(p => p.IsFinished);
                bool reached = Settings.method == TechMethod.HighestResearched
                    ? finished > 0
                    : (float)finished / projects.Count >= Settings.researchPercent;
                if (reached) return level;
            }
            return floor;
        }

        public static FactionRule DefaultRule(FactionDef def)
            => def == FactionDefOf.Empire || def == FactionDefOf.Insect ? FactionRule.Always : FactionRule.ByTech;

        public static FactionRule RuleFor(FactionDef def)
            => Settings.factionRules.TryGetValue(def.defName, out FactionRule rule) ? rule : DefaultRule(def);

        public static void SetRule(FactionDef def, FactionRule rule)
        {
            if (rule == DefaultRule(def)) Settings.factionRules.Remove(def.defName);
            else Settings.factionRules[def.defName] = rule;
        }

        public static bool LevelInRange(TechLevel level)
        {
            if (level == TechLevel.Undefined) return true;
            if (Settings.method == TechMethod.FixedRange)
                return (int)level >= Settings.fixedMin && (int)level <= Settings.fixedMax;
            int difference = (int)level - (int)ColonyLevel;
            return difference > 0
                ? Settings.levelsAhead < 0 || difference <= Settings.levelsAhead
                : Settings.levelsBehind < 0 || -difference <= Settings.levelsBehind;
        }

        public static bool Eligible(FactionDef def)
        {
            FactionRule rule = RuleFor(def);
            return rule == FactionRule.Always || rule == FactionRule.ByTech && LevelInRange(def.techLevel);
        }

        public static bool Eligible(Faction faction) => Eligible(faction.def);

        // How far a level is from the allowed range, for choosing the closest faction when none is in range
        private static int Distance(TechLevel level)
        {
            int value = (int)level;
            if (Settings.method != TechMethod.FixedRange) return Math.Abs(value - (int)ColonyLevel);
            return value < Settings.fixedMin ? Settings.fixedMin - value : Math.Max(0, value - Settings.fixedMax);
        }

        public static bool TryChooseRaidFaction(IncidentWorker_RaidEnemy worker, IncidentParms parms, out Faction? faction)
        {
            if (TryChoose(worker, parms, false, Eligible, out faction) || TryChoose(worker, parms, true, Eligible, out faction)) return true;
            if (!Settings.keepRaids) return false;
            List<Faction> remaining = Find.FactionManager.AllFactions
                .Where(f => RuleFor(f.def) != FactionRule.Never && worker.FactionCanBeGroupSource(f: f, parms: parms, desperate: true))
                .ToList();
            while (remaining.Count > 0)
            {
                int closest = remaining.Min(f => Distance(f.def.techLevel));
                List<Faction> nearest = remaining.Where(f => Distance(f.def.techLevel) == closest).ToList();
                if (TryChoose(worker, parms, true, nearest.Contains, out faction)) return true;
                remaining.RemoveAll(nearest.Contains);
            }
            faction = null;
            return false;
        }

        private static bool TryChoose(IncidentWorker_RaidEnemy worker, IncidentParms parms, bool desperate, Predicate<Faction> allowed, out Faction? faction)
            => PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroupWeighted(parms, out faction,
                f => allowed(f) && worker.FactionCanBeGroupSource(f: f, parms: parms, desperate: desperate),
                allowNonHostileToPlayer: true, allowHidden: true, allowDefeated: true);

        // Threats with no faction behind them follow the faction they belong to
        public static FactionDef? IncidentSource(IncidentDef incident)
        {
            Type worker = incident.workerClass;
            if (worker == null) return null;
            if (typeof(IncidentWorker_MechCluster).IsAssignableFrom(worker) || typeof(IncidentWorker_CrashedShipPart).IsAssignableFrom(worker))
                return FactionDefOf.Mechanoid;
            if (typeof(IncidentWorker_Infestation).IsAssignableFrom(worker) || typeof(IncidentWorker_DeepDrillInfestation).IsAssignableFrom(worker))
                return FactionDefOf.Insect;
            return null;
        }
    }
}
