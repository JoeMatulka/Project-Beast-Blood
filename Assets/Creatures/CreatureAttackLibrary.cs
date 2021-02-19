using CreatuePartSystems;
using CreatureSystems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureAttackLibrary
{
    // All creature attacks need to be added to this enum
    public enum CreatureAttackID
    {
        // Base Bipedal Attacks
        BIPEDAL_LOW_PUNCH = 1,
    }

    public static class BipedalCreatureBaseAttackLibrary
    {
        public static CreatureAttack LowPunch
        {
            get
            {
                return new CreatureAttack(
                    (int)CreatureAttackID.BIPEDAL_LOW_PUNCH,
                    new Dictionary<int, CreatureAttackFrame>
                    {
                        { 12, new CreatureAttackFrame(new string[] { "bicep_left", "forearm_left", "hand_left"}, 10f) },
                        { 15, new CreatureAttackFrame(new string[] { }, 0f) }
                    },
                    (in Vector2 targetPos, in Creature creature, in CreaturePart attackPart) =>
                    {
                        Vector2 creaturePos = creature.transform.localPosition;
                        return (creature.IsFacingRight && targetPos.x > creaturePos.x || (!creature.IsFacingRight && targetPos.x < creaturePos.x) && targetPos.y <= creaturePos.y);
                    }
                );
            }
        }
    }

    // Used to determine if the conditions are met for a creature to use a specific attack
    public delegate bool CreatureAttackCondition(in Vector2 targetPos, in Creature creature, in CreaturePart attackPart);
    // Used to calculate the damage of the creature attack, useful for setting affects from broken attack parts
    public delegate Damage CreatureAttackDamageCalculation(in Damage damage, in CreaturePart attackPart);

    public class CreatureAttack
    {
        private int id;
        private Dictionary<int, CreatureAttackFrame> frames;
        private CreatureAttackCondition attackCondition;
        private CreatureAttackDamageCalculation attackDmgCalc;
        // Part used to make the attack. If broken, it will affect the attack
        private CreaturePart attackPart;

        private Damage damage;

        public CreatureAttack(int id, Dictionary<int, CreatureAttackFrame> frames, CreatureAttackCondition attackCondition)
        {
            this.id = id;
            this.frames = frames;
            this.attackCondition = attackCondition;
        }

        public CreatureAttack(int id, Dictionary<int, CreatureAttackFrame> frames, CreatureAttackCondition attackCondition, CreatureAttackDamageCalculation attackDmgCalc, CreaturePart attackPart, Damage damage)
        {
            this.id = id;
            this.frames = frames;
            this.attackCondition = attackCondition;
            this.attackDmgCalc = attackDmgCalc;
            this.attackPart = attackPart;
            this.damage = damage;
        }

        public CreatureAttack SetAttackDamageCalculation(CreatureAttackDamageCalculation attackDmgCalc)
        {
            this.attackDmgCalc = attackDmgCalc;
            return this;
        }

        public CreatureAttack SetAttackPart(CreaturePart part)
        {
            this.attackPart = part;
            return this;
        }

        public CreaturePart AttackPart
        {
            get { return this.attackPart; }
        }

        public CreatureAttack SetDamage(Damage damage)
        {
            this.damage = damage;
            return this;
        }

        public Damage GetDamage()
        {
            Damage dmg = this.damage;
            if (attackDmgCalc != null)
            {
                dmg = attackDmgCalc(dmg, AttackPart);
            }
            return dmg;
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

