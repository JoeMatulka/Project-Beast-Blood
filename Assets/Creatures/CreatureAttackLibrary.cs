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
            // ID
            (int)CreatureAttackID.LOW_PUNCH,
            // Active Attack Frames
            new Dictionary<int, CreatureAttackFrame> {
                { 12, new CreatureAttackFrame(new string[] { "bicep_left", "forearm_left", "hand_left"}, 10f) },
                { 15, new CreatureAttackFrame(new string[] { }, 0f) }
            },
            // Attack Conditions
            (Vector2 targetPos, Creature creature) =>
            {
                CreatureAttack attack = null;
                Vector2 creaturePos = creature.transform.localPosition;
                if (((creature.IsFacingRight && targetPos.x > creaturePos.x) || (!creature.IsFacingRight && targetPos.x < creaturePos.x)) && targetPos.y <= creaturePos.y)
                {
                    // Creature is currently facing target and target is lower than creature
                    attack = LOW_PUNCH;
                    Damage dmg = new Damage(30, creature.Stats.AttackDamageType);
                    attack.Damage = dmg;
                }
                return attack;
            }
        );

        public static CreatureAttack[] BaseAttacks = new CreatureAttack[] {
            LOW_PUNCH,
        };
    }

    // Used to determine if the conditions are met for a creature to use a specific attack. Returns the attack if conditions are met
    public delegate CreatureAttack CreatureAttackCondition(Vector2 targetPos, Creature creature);

    public class CreatureAttack
    {
        private int id;
        private Dictionary<int, CreatureAttackFrame> frames;
        private CreatureAttackCondition attackCondition;

        public Damage Damage;

        public CreatureAttack(int id, Dictionary<int, CreatureAttackFrame> frames, CreatureAttackCondition attackCondition)
        {
            this.id = id;
            this.frames = frames;
            this.attackCondition = attackCondition;
        }

        public int ID
        {
            get { return id; }
        }

        public Dictionary<int, CreatureAttackFrame> Frames
        {
            get { return frames; }
        }

        public CreatureAttackCondition AttackCondition
        {
            get { return attackCondition; }
        }
    }
}

