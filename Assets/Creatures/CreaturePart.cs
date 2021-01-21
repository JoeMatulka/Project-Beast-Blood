﻿using CreatureSystems;
using HitboxSystem;
using UnityEngine;

namespace CreatuePartSystems
{
    public enum CreaturePartDamageModifier
    {
        NONE, KO, TRIP
    }

    /**
    * Class meant to represent a body part on the creature that registers damage on hit, can be breakable
    */
    public class CreaturePart : MonoBehaviour
    {
        public Creature creature;

        [SerializeField]
        public float PartHealth;
        [SerializeField]
        private bool IsBreakable;

        [SerializeField]
        private CreaturePartDamageModifier DamageModifier;

        public Hitbox[] hitBoxes;

        // Meant to allow certain parts to act as triggers to allow the player to easily navigate around the hit box
        public bool IsHitBoxTrigger = false;
        [SerializeField]
        private bool isBroken = false;
        // Damage Modifier Values for breaks and fresh breaks
        private const float DAMAGE_MOD_BASE = 1;
        private const float DAMAGE_MOD_BROKEN = 1.25f;
        private const float DAMAGE_MOD_FRESH_BREAK = 2;

        private void Start()
        {
            if (hitBoxes != null && hitBoxes.Length > 0)
            {
                foreach (Hitbox hitbox in hitBoxes)
                {
                    if (hitbox != null)
                    {
                        hitbox.Handler += new Hitbox.HitboxEventHandler(OnHit);
                        hitbox.Collider.isTrigger = IsHitBoxTrigger;
                    }
                    else
                    {
                        Debug.LogError(gameObject.name + " tried to set a hit box on limb that wasn't set");
                    }
                }
            }
            else
            {
                Debug.LogError(gameObject.name + " creature part was set without a hit box");
            }
            creature = GetComponentInParent<Creature>();
        }

        private void OnHit(object sender, HitboxEventArgs e)
        {
            Damage dmg = e.Damage;
            float dmgModAmount = DAMAGE_MOD_BASE;
            if (IsBreakable & !isBroken)
            {
                PartHealth -= dmg.Value;
                isBroken = PartHealth <= 0;
                dmgModAmount = DAMAGE_MOD_FRESH_BREAK;
            }
            if (isBroken) dmgModAmount = DAMAGE_MOD_BROKEN;
            creature.Damage(dmg, DamageModifier, dmgModAmount);
        }

        public bool IsBroken
        {
            get { return isBroken; }
        }
    }
}