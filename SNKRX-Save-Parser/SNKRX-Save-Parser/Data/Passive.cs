using SNKRX_Save_Parser.Attributes;

namespace SNKRX_Save_Parser
{
    /// <summary>
    /// Represents a saved passive.
    /// </summary>
    public record Passive
    {
        [SaveDataName("level")]
        public int Level { get; set; }
        [SaveDataName("xp")]
        public int Xp { get; set; }
        [SaveDataName("passive")]
        public PassiveName Name { get; set; }
    }
}