using CreatureSystems;
using HitboxSystem;
using ResourceManager;
using System.Collections;
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
        public float BurnBuildUp;
        private const float BURN_TIME = 5f;
        private const float BURN_RECOVER_RATE = 5f;
        private readonly Damage BURN_DMG = new Damage(10, DamageType.FIRE);
        [SerializeField]
        private bool IsBreakable;

        // If the part can apply Tripped or KO status to the creature
        [SerializeField]
        private CreaturePartDamageModifier DamageModifier;

        public Hitbox[] hitBoxes;

        // Meant to allow certain parts to act as triggers to allow the player to easily navigate around the hit box
        public bool IsHitBoxTrigger = false;
        [SerializeField]
        private bool isBroken = false;
        // Damage Modifier Values for breaks and fresh breaks
        private const float DAMAGE_MOD_BASE = 1;
        private const float DAMAGE_MOD_BROKEN = .5f;
        private const float DAMAGE_MOD_FRESH_BREAK = 2;

        private Renderer m_renderer;

        private void Awake()
        {
            m_renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            // Set up delegates on hitboxes for part
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
            // Ignore Collisions between ground in level and hitboxes
            GameObject[] ground = GameObject.FindGameObjectsWithTag("Ground");
            for (int i = 0; i < ground.Length; i++)
            {
                Collider2D collider = ground[i].GetComponent<Collider2D>();
                if (collider != null)
                {
                    // Iterate through attached hitboxes to ignore collisions with the ground collider
                    for (int ii = 0; ii < hitBoxes.Length; ii++)
                    {
                        Physics2D.IgnoreCollision(collider, hitBoxes[ii].Collider);
                    }
                }
            }
            // Start status build up reduce timers
            InvokeRepeating("ReduceBurnBuildUp", 1, 1);
        }

        private void OnHit(object sender, HitboxEventArgs e)
        {
            Damage dmg = e.Damage;
            float dmgModAmount = DAMAGE_MOD_BASE;
            // Apply part damage is part is breakable and break it if part health is depleted
            if (IsBreakable & !isBroken)
            {
                PartHealth -= dmg.Value;
                isBroken = PartHealth <= 0;
                if (isBroken)
                {
                    dmgModAmount = DAMAGE_MOD_FRESH_BREAK;
                    ApplyBrokenEffectsToPart();
                }
            }
            // Apply Damage modifier if part is broken, used to deter the player from attacking the same part the whole fight
            if (isBroken) dmgModAmount = DAMAGE_MOD_BROKEN;
            // If incoming damage is FIRE, apply buring build up and burn status if threshold is met
            if (dmg.Type.Equals(DamageType.FIRE))
            {
                BurnBuildUp += dmg.Value;
                if (BurnBuildUp >= creature.Stats.BurnThreshold) ApplyBurningEffectsToPart();
            }

            creature.Damage(dmg, DamageModifier, dmgModAmount);
        }

        private void ApplyBrokenEffectsToPart()
        {
            Vector2 effectPos = this.transform.position;
            Quaternion effectRot = this.transform.rotation;
            Transform effectParent = this.transform;
            Color bloodColor = creature.BloodColor;
            // Create Blood Splash
            GameObject splash = EffectsManager.Instance.BloodSplash;
            splash.GetComponent<ParticleSystem>().startColor = bloodColor;
            Instantiate(splash, effectPos, effectRot, effectParent);
            // Create Bleeding Effect
            GameObject bleeding = EffectsManager.Instance.Bleeding;
            bleeding.GetComponent<ParticleSystem>().startColor = bloodColor;
            Instantiate(bleeding, effectPos, effectRot, effectParent);
            // Apply bloodied material
            m_renderer.material = EffectsManager.Instance.BloodiedMaterial;
            m_renderer.material.SetColor("_Color", bloodColor);
        }

        private void ApplyBurningEffectsToPart()
        {
            // TODO Create Burn Effect here on part
            StartCoroutine(Burn());
        }

        private IEnumerator Burn()
        {
            float startBurnTime = Time.time;
            creature.isBurning = true;
            while ((Time.time - startBurnTime) <= BURN_TIME)
            {
                yield return new WaitForSeconds(1);
                creature.Damage(BURN_DMG, DamageModifier);
            }
            creature.isBurning = false;
        }

        private void ReduceBurnBuildUp()
        {
            if (BurnBuildUp >= 0)
            {
                BurnBuildUp -= BURN_RECOVER_RATE;
            }
            else
            {
                BurnBuildUp = 0;
            }
        }

        public bool IsBroken
        {
            get { return isBroken; }
        }

        public Creature Creature
        {
            get { return creature; }
        }
    }
}