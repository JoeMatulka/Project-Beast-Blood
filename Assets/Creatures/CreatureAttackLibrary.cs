using CreatureSystems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureAttackLibrary
{
    public enum CreatureAttackID
    {
        LOW_PUNCH = 1,
    }

    public static class BaseBipedalCreatureAttackLibrary
    {
        public static CreatureAttack LOW_PUNCH = new CreatureAttack(
            (int)CreatureAttackID.LOW_PUNCH,
            new Dictionary<int, CreatureAttackFrame> {
                { 12, new CreatureAttackFrame(new string[] { "bicep_left", "forearm_left", "hand_left"}, 10f) },
                { 15, new CreatureAttackFrame(new string[] { }, 0f) }
            });
    }

    public class CreatureAttack
    {
        private int id;
        private Dictionary<int, CreatureAttackFrame> frames;
        public Damage Damage;

        public CreatureAttack(int id, Dictionary<int, CreatureAttackFrame> frames)
        {
            this.id = id;
            this.frames = frames;
        }

        public int ID
        {
            get { return id; }
        }

        public Dictionary<int, CreatureAttackFrame> Frames
        {
            get { return frames; }
        }
    }
}

