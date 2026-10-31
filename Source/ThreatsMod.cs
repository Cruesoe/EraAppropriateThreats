using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EraAppropriateThreats
{
    public class ThreatsMod : Mod
    {
        public static ThreatSettings Settings = null!;

        private static Vector2 scroll;
        private static float viewHeight = 1200f;
        private static List<FactionDef>? factionDefs;

        public ThreatsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ThreatSettings>();
            ApplyPatches();
        }

        // Each class is patched on its own, so a changed optional mod only loses its own patch
        private static void ApplyPatches()
        {
            Harmony harmony = new Harmony("cruesoe.eraappropriatethreats");
            foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(ThreatsMod).Assembly))
            {
                if (!type.IsDefined(typeof(HarmonyPatch), false)) continue;
                try
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception e)
                {
                    Log.Warning("[Era Appropriate Threats] Could not apply " + type.Name + ": " + e.Message);
                }
            }
        }

        public override string SettingsCategory() => "Era Appropriate Threats";

        public override void WriteSettings()
        {
            base.WriteSettings();
            TechPolicy.Invalidate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect view = new Rect(0f, 0f, inRect.width - 20f, viewHeight);
            Widgets.BeginScrollView(inRect, ref scroll, view);
            Listing_Standard listing = new Listing_Standard { maxOneColumn = true };
            listing.Begin(view);
            DrawStatus(listing);
            TechMethod method = Settings.method;
            float percent = Settings.researchPercent;
            DrawMethod(listing);
            if (Settings.method != method || Settings.researchPercent != percent) TechPolicy.Invalidate();
            listing.GapLine();
            DrawFilters(listing);
            listing.GapLine();
            DrawFactions(listing);
            viewHeight = listing.CurHeight + 12f;
            listing.End();
            Widgets.EndScrollView();
        }

        private static void DrawStatus(Listing_Standard listing)
        {
            if (!TechPolicy.InGame)
            {
                listing.Label("EAT_NotInGame".Translate());
                listing.GapLine();
                return;
            }
            if (Settings.method != TechMethod.FixedRange)
                listing.Label("EAT_ColonyLevel".Translate(TechPolicy.ColonyLevel.ToStringHuman()));
            IEnumerable<string> hostile = Find.FactionManager.AllFactions
                .Where(f => !f.IsPlayer && !f.defeated && !f.deactivated && !f.temporary && f.HostileTo(Faction.OfPlayer) && CanRaid(f.def) && TechPolicy.Eligible(f))
                .Select(f => f.Name);
            string names = hostile.ToCommaList();
            listing.Label("EAT_EligibleRaiders".Translate(names.NullOrEmpty() ? "EAT_None".Translate().ToString() : names));
            listing.Label("EAT_TemperatureNote".Translate());
            listing.GapLine();
        }

        private static void DrawMethod(Listing_Standard listing)
        {
            listing.Label("EAT_MethodHeader".Translate());
            Method(listing, TechMethod.ResearchPercent, "EAT_MethodPercent", "EAT_MethodPercentTip");
            if (Settings.method == TechMethod.ResearchPercent)
            {
                listing.Label("EAT_Percent".Translate(Settings.researchPercent.ToStringPercent()));
                Settings.researchPercent = Mathf.Round(listing.Slider(Settings.researchPercent, 0.05f, 1f) * 20f) / 20f;
            }
            Method(listing, TechMethod.HighestResearched, "EAT_MethodHighest", "EAT_MethodHighestTip");
            Method(listing, TechMethod.ColonyLevel, "EAT_MethodColony", "EAT_MethodColonyTip");
            Method(listing, TechMethod.FixedRange, "EAT_MethodFixed", "EAT_MethodFixedTip");
            listing.Gap();
            if (Settings.method == TechMethod.FixedRange)
            {
                listing.Label("EAT_FixedMin".Translate(((TechLevel)Settings.fixedMin).ToStringHuman()));
                Settings.fixedMin = Mathf.RoundToInt(listing.Slider(Settings.fixedMin, 1f, 7f));
                listing.Label("EAT_FixedMax".Translate(((TechLevel)Settings.fixedMax).ToStringHuman()));
                Settings.fixedMax = Mathf.Max(Settings.fixedMin, Mathf.RoundToInt(listing.Slider(Settings.fixedMax, 1f, 7f)));
                return;
            }
            listing.Label("EAT_LevelsAhead".Translate(LevelsLabel(Settings.levelsAhead)));
            Settings.levelsAhead = Mathf.RoundToInt(listing.Slider(Settings.levelsAhead, -1f, 6f));
            listing.Label("EAT_LevelsBehind".Translate(LevelsLabel(Settings.levelsBehind)));
            Settings.levelsBehind = Mathf.RoundToInt(listing.Slider(Settings.levelsBehind, -1f, 6f));
        }

        private static void Method(Listing_Standard listing, TechMethod method, string label, string tip)
        {
            if (listing.RadioButton(label.Translate(), Settings.method == method, 0f, tip.Translate()))
                Settings.method = method;
        }

        private static string LevelsLabel(int levels) => levels < 0 ? "EAT_Any".Translate().ToString() : levels.ToString();

        private static void DrawFilters(Listing_Standard listing)
        {
            listing.CheckboxLabeled("EAT_FilterRaids".Translate(), ref Settings.filterRaids, "EAT_FilterRaidsTip".Translate());
            RaidOption(listing, "EAT_KeepRaids", "EAT_KeepRaidsTip", ref Settings.keepRaids);
            DrawOutOfRangeChance(listing);
            listing.CheckboxLabeled("EAT_FilterIncidents".Translate(), ref Settings.filterIncidents, "EAT_FilterIncidentsTip".Translate());
            listing.CheckboxLabeled("EAT_FilterQuestSites".Translate(), ref Settings.filterQuestSites, "EAT_FilterQuestSitesTip".Translate());
            RaidOption(listing, "EAT_ReplaceQuestRaiders", "EAT_ReplaceQuestRaidersTip", ref Settings.replaceQuestRaiders);
            listing.CheckboxLabeled("EAT_FilterVisitors".Translate(), ref Settings.filterVisitors, "EAT_FilterVisitorsTip".Translate());
        }

        // Options that only work with Limit raids on are greyed out without it
        private static void RaidOption(Listing_Standard listing, string label, string tip, ref bool value)
        {
            Rect row = listing.GetRect(Text.LineHeight);
            listing.Gap(listing.verticalSpacing);
            Widgets.CheckboxLabeled(row, label.Translate(), ref value, !Settings.filterRaids);
            TooltipHandler.TipRegion(row, (Settings.filterRaids ? tip : "EAT_RequiresLimitRaids").Translate());
        }

        private static void DrawOutOfRangeChance(Listing_Standard listing)
        {
            string tip = (Settings.filterRaids ? "EAT_OutOfRangeChanceTip" : "EAT_RequiresLimitRaids").Translate();
            Color color = GUI.color;
            if (!Settings.filterRaids) GUI.color = Color.gray;
            listing.Label("EAT_OutOfRangeChance".Translate(Settings.outOfRangeChance.ToStringPercent()), tooltip: tip);
            float chance = Mathf.Round(listing.Slider(Settings.outOfRangeChance, 0f, 1f) * 20f) / 20f;
            GUI.color = color;
            if (Settings.filterRaids) Settings.outOfRangeChance = chance;
        }

        private static void DrawFactions(Listing_Standard listing)
        {
            listing.Label("EAT_FactionsHeader".Translate());
            factionDefs ??= DefDatabase<FactionDef>.AllDefs
                .Where(d => !d.isPlayer && CanRaid(d))
                .OrderBy(d => d.techLevel).ThenBy(d => d.defName)
                .ToList();
            foreach (FactionDef def in factionDefs)
            {
                Rect row = listing.GetRect(30f);
                string label = def.label.NullOrEmpty() ? def.defName : def.LabelCap.ToString();
                Widgets.Label(row.LeftPartPixels(row.width - 150f), label + " (" + def.techLevel.ToStringHuman() + ")");
                FactionRule rule = TechPolicy.RuleFor(def);
                if (Widgets.ButtonText(row.RightPartPixels(140f), ("EAT_Rule" + rule).Translate()))
                    TechPolicy.SetRule(def, (FactionRule)(((int)rule + 1) % 3));
            }
            listing.Gap();
            if (listing.ButtonText("EAT_ResetRules".Translate())) Settings.factionRules.Clear();
        }

        private static bool CanRaid(FactionDef def)
            => def.pawnGroupMakers != null && def.pawnGroupMakers.Any(m => m.kindDef == PawnGroupKindDefOf.Combat);
    }
}
