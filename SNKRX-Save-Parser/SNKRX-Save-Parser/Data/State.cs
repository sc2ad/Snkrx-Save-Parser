using SNKRX_Save_Parser.Attributes;
using System.Collections.Generic;

namespace SNKRX_Save_Parser
{
    /// <summary>
    /// Represents a locked state window.
    /// </summary>
    public record State
    {
        [SaveDataName("locked")]
        [SerializeIf(true)]
        public bool Locked { get; set; } = false;
        [SaveDataName("cards")]
        public List<CharacterName> Cards { get; private set; } = new List<CharacterName>();
    }
}