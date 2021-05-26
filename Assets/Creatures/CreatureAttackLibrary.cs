using CreatuePartSystems;
using CreatureSystems;
using ResourceManager;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureAttackLibrary
{
    // All creature attacks need to be added to this enum
    public enum CreatureAttackID
    {
        // Base Bipedal Attacks
        BIPEDAL_LOW_PUNCH = 1,
        BIPEDAL_DOWNWARD_SLAM = 2,
        BIPEDAL_ROAR = 3,
    }

    public enum CreatureEffectID
    {
        NONE,
        ROAR,
    }

    public static class CreatureAttackEffectLoader
    {
        public static GameObject LoadEffect(CreatureEffectID id) {
            switch (id) {
                case CreatureEffectID.ROAR:
                    return EffectsManager.Instance.Roar;
                case CreatureEffectID.NONE:
                default:
                    return null;
            }
        }
    }

    public static class BipedalCreatureBaseAttackLibrary
    {
        private const float DOWNWARD_ATK_DISTANCE = 1.5f;

        private static CreatureAttackDamageCalculation DEF_ATK_DMG_CALC = (in Damage damage, in CreaturePart attackPart) =>
        {
            Damage dmg = damage;
            // Reduce damage when attack part is broken
            if (attackPart.IsBroken) { dmg = new Damage(damage.Value / 1.5f, damage.Type); }
            return dmg;
        };

        public static CreatureAttack LowPunch
        {
            get
            {
                return new CreatureAttack(
                    (int)CreatureAttackID.BIPEDAL_LOW_PUNCH,
                    new Dictionary<int, CreatureAttackFrame>
                    {
                        { 10, new CreatureAttackFrame(new string[] { }, new CreatureAttackSpriteSwap[] { new CreatureAttackSpriteSwap("hand_left", "Left Hand", "closed") }) },
                        { 13, new CreatureAttackFrame(new string[] { "bicep_left", "forearm_left", "hand_left"}, null, 10f) },
                        { 16, new CreatureAttackFrame(new string[] { }) },
                        { 23, new CreatureAttackFrame(new string[] { }, new CreatureAttackSpriteSwap[] { new CreatureAttackSpriteSwap("hand_left", "Left Hand", "open") }) }
                    },
                    (in Vector2 targetPos, in Creature creature, in CreaturePart attackPart) =>
                    {
                        Vector2 creaturePos = creature.transform.localPosition;
                        // Creature is facing target and it is on the lower y axis
                        return (creature.CheckGrounded() && creature.IsFacingRight && targetPos.x > creaturePos.x || (!creature.IsFacingRight && targetPos.x < creaturePos.x) && targetPos.y <= creaturePos.y);
                    },
                    DEF_ATK_DMG_CALC
                );
            }
        }

        public static CreatureAttack DownwardSlam
        {
            get
            {
                return new CreatureAttack(
                    (int)CreatureAttackID.BIPEDAL_DOWNWARD_SLAM,
                    new Dictionary<int, CreatureAttackFrame>
                    {
                        { 8, new CreatureAttackFrame(new string[] { },  new CreatureAttackSpriteSwap[] { new CreatureAttackSpriteSwap("hand_left", "Left Hand", "closed"), new CreatureAttackSpriteSwap("hand_right", "Right Hand", "closed") }) },
                        { 13, new CreatureAttackFrame(new string[] { "bicep_left", "forearm_left", "hand_left", "bicep_right", "forearm_right", "hand_right"}) },
                        { 16, new CreatureAttackFrame(new string[] { "bicep_left", "forearm_left", "hand_left"}) },
                        { 20, new CreatureAttackFrame(new string[] { }, new CreatureAttackSpriteSwap[] { new CreatureAttackSpriteSwap("hand_left", "Left Hand", "open"), new CreatureAttackSpriteSwap("hand_right", "Right Hand", "open") }) }
                    },
                    (in Vector2 targetPos, in Creature creature, in CreaturePart attackPart) =>
                    {
                        Vector2 creaturePos = creature.transform.localPosition;
                        float distToTarget = Vector2.Distance(creaturePos, targetPos);
                        // Target is beneath and close to creature
                        return (creature.CheckGrounded() && distToTarget <= DOWNWARD_ATK_DISTANCE && targetPos.y <= creaturePos.y);
                    },
                    DEF_ATK_DMG_CALC
                );
            }
        }

        public static CreatureAttack Roar
        {
            get
            {
                return new CreatureAttack(
                    (int)CreatureAttackID.BIPEDAL_ROAR,
                    new Dictionary<int, CreatureAttackFrame>
                    {
                        { 9, new CreatureAttackFrame(new string[] { "head" },  new CreatureAttackSpriteSwap[] { new CreatureAttackSpriteSwap("head", "Head", "roar") }) },
                        { 10, new CreatureAttackFrame(new string[] { }, CreatureEffectID.ROAR ) },
                        { 11, new CreatureAttackFrame(new string[] { } ) },
                        { 17, new CreatureAttackFrame(new string[] { },  new CreatureAttackSpriteSwap[] { new CreatureAttackSpriteSwap("head", "Head", "default") }) },
                    },
                    (in Vector2 targetPos, in Creature creature, in CreaturePart attackPart) =>
                    {
                        // TODO expand on this condition in the future if necessary
                        return false;
                    },
                    (in Damage damage, in CreaturePart attackPart) =>
                    {
                        return damage;
                    },
                    new Damage(0, DamageType.RAW)
                );
            }
        }
    }

    // Used to determine if the conditions are met for a creature to use a specific attack
    public delegate bool CreatureAttackCondition(in Vector2 targetPos, in Creature creature, in CreaturePart attackPart);
    // Used to calculate the damage of the creature attack, useful for setting affects from broken attack parts
    public delegate Damage CreatureAttackDamageCalculation(in Damage damage, in CreaturePart attackPart);

    public class CreatureAttackSpriteSwap
    {
        public readonly string Key;
        // Used for swapping the sprite resolver from the sprite library
        public readonly string Category;
        public readonly string Label;
        public CreatureAttackSpriteSwap(string key, string category, string label)
        {
            Key = key;
            Category = category;
            Label = label;
        }
    }

    public class CreatureAttack
    {
        private int id;
        private Dictionary<int, CreatureAttackFrame> frames;
        private CreatureAttackCondition attackCondition;
        private CreatureAttackDamageCalculation attackDmgCalc;
        // Part used to make the attack. If broken, it will affect the attack
        private CreaturePart attackPart;

        private Damage damage;

        public CreatureAttack(int id, Dictionary<int, CreatureAttackFrame> frames, CreatureAttackCondition attackCondition, CreatureAttackDamageCalculation attackDmgCalc)
        {
            this.id = id;
            this.frames = frames;
            this.attackCondition = attackCondition;
            this.attackDmgCalc = attackDmgCalc;
        }

        public CreatureAttack(int id, Dictionary<int, CreatureAttackFrame> frames, CreatureAttackCondition attackCondition, CreatureAttackDamageCalculation attackDmgCalc, Damage damage)
        {
            this.id = id;
            this.frames = frames;
            this.attackCondition = attackCondition;
            this.attackDmgCalc = attackDmgCalc;
            this.damage = damage;
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

        public void GenerateNewDamageGuid()
        {
            this.damage.GenerateNewGuid();
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

