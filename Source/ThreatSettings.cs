using System.Collections.Generic;
using Verse;

namespace EraAppropriateThreats
{
    public enum TechMethod
    {
        ResearchPercent,
        HighestResearched,
        ColonyLevel,
        FixedRange
    }

    public enum FactionRule
    {
        ByTech,
        Always,
        Never
    }

    public class ThreatSettings : ModSettings
    {
        public TechMethod method = TechMethod.ResearchPercent;
        public float researchPercent = 0.5f;
        // -1 means any number of levels
        public int levelsAhead = 1;
        public int levelsBehind = -1;
        public int fixedMin = 1;
        public int fixedMax = 7;
        public bool filterRaids = true;
        public bool keepRaids = true;
        public float outOfRangeChance;
        public bool filterIncidents = true;
        public bool filterQuestSites = true;
        public bool replaceQuestRaiders;
        public bool filterVisitors;
        // Only rules that differ from the defaults, by FactionDef defName
        public Dictionary<string, FactionRule> factionRules = new Dictionary<string, FactionRule>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref method, "method", TechMethod.ResearchPercent);
            Scribe_Values.Look(ref researchPercent, "researchPercent", 0.5f);
            Scribe_Values.Look(ref levelsAhead, "levelsAhead", 1);
            Scribe_Values.Look(ref levelsBehind, "levelsBehind", -1);
            Scribe_Values.Look(ref fixedMin, "fixedMin", 1);
            Scribe_Values.Look(ref fixedMax, "fixedMax", 7);
            Scribe_Values.Look(ref filterRaids, "filterRaids", true);
            Scribe_Values.Look(ref keepRaids, "keepRaids", true);
            Scribe_Values.Look(ref outOfRangeChance, "outOfRangeChance", 0f);
            Scribe_Values.Look(ref filterIncidents, "filterIncidents", true);
            Scribe_Values.Look(ref filterQuestSites, "filterQuestSites", true);
            Scribe_Values.Look(ref replaceQuestRaiders, "replaceQuestRaiders", false);
            Scribe_Values.Look(ref filterVisitors, "filterVisitors", false);
            Scribe_Collections.Look(ref factionRules, "factionRules", LookMode.Value, LookMode.Value);
            factionRules ??= new Dictionary<string, FactionRule>();
        }
    }
}
