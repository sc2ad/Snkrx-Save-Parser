using SNKRX_Save_Parser.Attributes;
using System.Collections.Generic;

namespace SNKRX_Save_Parser
{
    /// <summary>
    /// Represents a saved unit.
    /// </summary>
    public record Character
    {
        [SaveDataName("level")]
        public int Level { get; set; }
        [SaveDataName("character")]
        public CharacterName Name { get; set; }
        [SaveDataName("spawn_effect")]
        public bool SpawnEffect { get; set; }
        [SaveDataName("reserve")]
        public List<int> Reserve { get; private set; } = new();
    }
}