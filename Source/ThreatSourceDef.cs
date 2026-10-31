using RimWorld;
using Verse;

namespace EraAppropriateThreats
{
    // Ties a threat event to a faction's rule, a tech level, or both: with both, Never blocks it and otherwise the tech level decides
    public class ThreatSourceDef : Def
    {
        public string incident = "";
        public string faction = "";
        public TechLevel techLevel = TechLevel.Undefined;
    }
}
