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
        private readonly Damage BURN_DMG = new Damage(10, DamageElementType.FIRE);
        [SerializeField]
        public float PoisonBuildUp;
        private const float POISON_TIME = 20f;
        private readonly Damage POISON_DMG = new Damage(2.5f, DamageElementType.POISON);
        private const float STATUS_RECOVER_RATE = 5f;
        private bool isTakingStatusDamage = false;
        [SerializeField]
        public bool IsBreakable;

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
        private Material defMaterial;

        private void Awake()
        {
            m_renderer = GetComponent<Renderer>();
            defMaterial = m_renderer.material;
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
            InvokeRepeating("ReduceStatusBuildUp", 1, 1);
        }

        private void OnHit(object sender, HitboxEventArgs e)
        {
            // Return if there is no damage value
            if (e.Damage.Value <= 0) return;
            // Apply part damage is part is breakable and break it if part health is depleted
            float dmgModAmount = DAMAGE_MOD_BASE;
            if (IsBreakable & !isBroken)
            {
                // Sharp modifier increases damage to parts
                float partDamage = e.Damage.Mods[DamageModType.SHARP] * e.Damage.Value;
                PartHealth -= partDamage;
                isBroken = PartHealth <= 0;
                // If hit breaks part, apply effects to part
                if (isBroken)
                {
                    dmgModAmount = DAMAGE_MOD_FRESH_BREAK;
                    ApplyBrokenEffectsToPart();
                }
            }
            // Apply Damage modifier if part is broken, used to deter the player from attacking the same part the whole fight
            if (isBroken) dmgModAmount = DAMAGE_MOD_BROKEN;
            // If incoming damage is FIRE, apply buring build up and burn status if threshold is met
            if (e.Damage.Type.Equals(DamageElementType.FIRE) && !creature.isBurning)
            {
                // Spawn fire spark for hit effect
                GameObject spark = EffectsManager.Instance.Spark;
                Instantiate(spark, e.Damage.Position, Quaternion.identity, this.transform);
                BurnBuildUp += e.Damage.Value;
                if (BurnBuildUp >= creature.Stats.BurnThreshold) ApplyBurningEffectsToPart();
            }
            // If incoming damage is Poison, apply poisoned build up and poison status if threshold is met
            if (e.Damage.Type.Equals(DamageElementType.POISON))
            {
                // Spawn poison puff for hit effect
                GameObject puff = EffectsManager.Instance.PoisonPuff;
                Instantiate(puff, e.Damage.Position, Quaternion.identity, this.transform);
                PoisonBuildUp += e.Damage.Value;
                if (PoisonBuildUp >= creature.Stats.PoisonThreshold) ApplyPoisonedEffectsToPart();
            }
            // Spawn small blood splash for hit effect
            if (dmgModAmount != DAMAGE_MOD_BROKEN)
            {
                // Only apply blood splash if part is not broken to give some feedback that not as much damage is being done, damage numbers may help this as well
                GameObject splash = EffectsManager.Instance.BloodSplashSmall;
                ParticleSystem.MainModule splashSettings = splash.GetComponent<ParticleSystem>().main;
                splashSettings.startColor = creature.BloodColor;
                Instantiate(splash, e.Damage.Position, Quaternion.identity, this.transform);
            }
            // Damage part
            creature.Damage(e.Damage, DamageModifier, dmgModAmount);
        }

        private void ApplyBrokenEffectsToPart()
        {
            Vector2 effectPos = this.transform.position;
            Quaternion effectRot = this.transform.rotation;
            Transform effectParent = this.transform;
            Color bloodColor = creature.BloodColor;
            // Create Blood Splash
            GameObject splash = EffectsManager.Instance.BloodSplashLarge;
            ParticleSystem.MainModule splashSettings = splash.GetComponent<ParticleSystem>().main;
            splashSettings.startColor = bloodColor;
            Instantiate(splash, effectPos, effectRot, effectParent);
            // Create Bleeding Effect
            GameObject bleeding = EffectsManager.Instance.Bleeding;
            ParticleSystem.MainModule bleedSettings = splash.GetComponent<ParticleSystem>().main;
            bleedSettings.startColor = bloodColor;
            Instantiate(bleeding, effectPos, effectRot, effectParent);
            // Apply bloodied material, don't apply while recieving status damage to remove the current effect
            if (!isTakingStatusDamage)
            {
                ApplyMaterialToChildRenderers(EffectsManager.Instance.BloodiedMaterial);
                m_renderer.material.SetColor("_Color", bloodColor);
            }
            // Add this as the default material from here on out so when burns and poisons stop, they don't remove this effect
            defMaterial = m_renderer.material;
        }

        private void ApplyBurningEffectsToPart()
        {
            Vector2 effectPos = this.transform.position;
            Quaternion effectRot = this.transform.rotation;
            Transform effectParent = this.transform;
            // Create fire burning effect
            GameObject burning = Instantiate(EffectsManager.Instance.FireBurning, effectPos, effectRot, effectParent);
            ApplyMaterialToChildRenderers(EffectsManager.Instance.BurningMaterial);
            StartCoroutine(TakeStatusDamage(BURN_DMG, BURN_TIME, burning));
        }

        private void ApplyPoisonedEffectsToPart()
        {
            ApplyMaterialToChildRenderers(EffectsManager.Instance.PoisonedMaterial);
            StartCoroutine(TakeStatusDamage(POISON_DMG, POISON_TIME));
        }

        private IEnumerator TakeStatusDamage(Damage dmg, float statusTime, GameObject effect = null)
        {
            isTakingStatusDamage = true;
            float startTime = Time.time;
            if (dmg.Type.Equals(DamageElementType.FIRE)) creature.isBurning = true;
            if (dmg.Type.Equals(DamageElementType.POISON)) creature.isPoisoned = true;
            while ((Time.time - startTime) <= statusTime)
            {
                yield return new WaitForSeconds(1);
                creature.Damage(dmg, DamageModifier);
            }
            if (dmg.Type.Equals(DamageElementType.FIRE))
            {
                creature.isBurning = false;
                BurnBuildUp = 0;
            }
            if (dmg.Type.Equals(DamageElementType.POISON))
            {
                creature.isPoisoned = false;
                PoisonBuildUp = 0;
            }
            if (effect != null) Destroy(effect);
            isTakingStatusDamage = false;
            ApplyMaterialToChildRenderers(defMaterial);
        }

        private void ApplyMaterialToChildRenderers(Material material)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) { renderer.material = material; }
        }

        private void ReduceStatusBuildUp()
        {
            if (BurnBuildUp >= 0)
            {
                BurnBuildUp -= STATUS_RECOVER_RATE;
            }
            else
            {
                BurnBuildUp = 0;
            }
            if (PoisonBuildUp >= 0)
            {
                PoisonBuildUp -= STATUS_RECOVER_RATE;
            }
            else
            {
                PoisonBuildUp = 0;
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