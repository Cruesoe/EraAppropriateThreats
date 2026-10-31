using RimWorld;
using Verse;

namespace EraAppropriateThreats
{
    // Ties a threat event that chooses its own faction to a faction's rule, or to a tech level when no one faction fits
    public class ThreatSourceDef : Def
    {
        public string incident = "";
        public string faction = "";
        public TechLevel techLevel = TechLevel.Undefined;
    }
}
