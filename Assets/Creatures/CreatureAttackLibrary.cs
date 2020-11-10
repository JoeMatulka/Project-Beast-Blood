using System.Collections.Generic;

namespace CreatureAttackLibrary
{
    public static class BipedalCreatureAttackLibrary
    {
        public static Dictionary<int, CreatureAttackFrame> LOW_PUNCH_FRAMES = new Dictionary<int, CreatureAttackFrame> {
            { 12, new CreatureAttackFrame(new string[] { "bicep_left", "forearm_left", "hand_left"}, 25f) },
            { 15, new CreatureAttackFrame(new string[] { }, 0f) }
        };
    }
}

