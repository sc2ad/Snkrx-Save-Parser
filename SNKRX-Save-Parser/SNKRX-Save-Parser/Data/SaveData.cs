using SNKRX_Save_Parser.Attributes;
using System;
using System.Collections.Generic;

namespace SNKRX_Save_Parser
{
    /// <summary>
    /// Represents the entire collection of save data.
    /// </summary>
    public class SaveData
    {
        [SaveDataName("level")]
        public int Level { get; set; }

        [SaveDataName("shop_xp")]
        public int ShopXp { get; set; }

        [SaveDataName("passives")]
        public List<Passive> Passives { get; private set; } = new List<Passive>();

        [SaveDataName("gold")]
        public int Gold { get; set; }

        [SaveDataName("units")]
        public List<Character> Units { get; private set; } = new List<Character>();

        [SaveDataName("locked_state")]
        public State LockedState { get; private set; } = new State();

        [SaveDataName("shop_level")]
        public int ShopLevel { get; set; }

        [SaveDataName("run_passive_pool")]
        public List<PassiveName> RunPassivePool { get; private set; } = new List<PassiveName>();
    }
}