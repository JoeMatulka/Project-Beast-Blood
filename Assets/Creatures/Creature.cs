using Gamekit2D;
using HitboxSystem;
using CreatuePartSystems;
using UnityEngine;
using System.Collections;
using CreatureAttackLibrary;
using UnityEngine.Experimental.U2D.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using ResourceManager;

namespace CreatureSystems
{
    // Meant to act as a base classification for a creature
    public enum CreatureType
    {
        Bipedal
    }

    public enum CreaturePartsType
    {
        Ground, Flight
    }

    public enum CreatureOnEffect
    {
        // Creates a splash of blood
        BloodSplash,
        // Creates a short continuos spray of blood
        BloodSpurt
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteLibrary))]
    public abstract class Creature : MonoBehaviour
    {
        public struct CreatureStats
        {
            public string Name;
            public float BaseHealth;
            public float TripThreshold;
            public float KOThreshold;
            public float BurnThreshold;
            public float PoisonThreshold;
            public float Speed;
            // How often a creature is going to attack while in attack behavior
            public float BaseAggression;
            // Not meant to be the actual size in game, just representative of size in the fantasy term (in feet).
            public float BaseSize;
            public float SizeModifier;
            public CreatureType CreatureType;
            // Resisted element and the resistance value to it
            public Dictionary<DamageElementType, float> ResistedElements;
        }

        public Creature.CreatureStats Stats;

        public Color BloodColor;

        [SerializeField]
        protected float CurrentHealth;
        [SerializeField]
        private bool isDead = false;
        [SerializeField]
        protected float CurrentTripThreshold = 0;
        [SerializeField]
        protected float CurrentKOThreshold = 0;
        [SerializeField]
        protected float CurrentAgression = 0;
        [SerializeField]
        protected CreaturePart[] GroundMobilityParts;
        [SerializeField]
        protected CreaturePart[] FlightMobilityParts;
        [SerializeField]
        protected CreaturePart[] AttackParts;
        [SerializeField]
        private LayerMask GroundLayerMask;
        [SerializeField]
        private Transform groundCheck;

        private readonly float GROUND_RADIUS = .5f;

        private Vector3 velocity = Vector3.zero;

        private bool isFacingRight = false;
        [SerializeField]
        private bool isTripped = false;
        private const float TRIPPED_DOWN_TIME = 5f;
        [SerializeField]
        private bool isKnockedOut = false;
        private const float KNOCK_OUT_DOWN_TIME = 8f;
        [SerializeField]
        private bool isStaggered = false;
        private const float STAGGER_TIME = 3.5f;
        private const float STAGGER_CHANCE = 4.5f;
        public bool isBurning = false;
        public bool isPoisoned = false;

        protected Rigidbody2D m_Rigidbody;
        protected CircleCollider2D m_Collider;

        protected Animator animator;

        public Transform Target;
        public Vector2 LastKnownTargetPos;
        private Hitbox[] hitboxes;
        private CreatureAttack[] attackSet;
        protected CreatureAttack currentAttack;
        private Dictionary<int, CreatureAttackFrame> ActiveAttackFrames = new Dictionary<int, CreatureAttackFrame>();

        protected CreatureAiStateMachine aiStateMachine = new CreatureAiStateMachine();

        // Used for animations for landing, preparing to jump, getting up, etc. Anything that is a transition animation
        protected string[] transitionAnimations = new string[] { "Land", "Jump" };
        public bool isInAnimationTransition = false;

        public CreatureJumpEvent jumpEvent;
        private const float JUMP_DURATION = 0.5f;
        private bool isJumping = false;
        private bool hasInitiatedJump = false;
        private readonly Damage JUMPING_DOWN_DMG = new Damage(50, DamageElementType.RAW);

        public bool IsFleeing = false;
        public float TimeSinceLastFlee = 0f;

        public readonly static float WALK_INPUT = .6f;
        public readonly static float RUN_INPUT = 1.5f;

        /**
         * Should be called in Awake phase of a creature object 
         **/
        protected void InitialSetUp(Creature.CreatureStats stats, CreatureAttack[] attackSet)
        {
            m_Rigidbody = this.GetComponent<Rigidbody2D>();
            m_Rigidbody.freezeRotation = true;
            m_Rigidbody.mass = 25;
            m_Collider = this.GetComponent<CircleCollider2D>();
            hitboxes = this.GetComponentsInChildren<Hitbox>();

            animator = this.GetComponent<Animator>();
            SceneLinkedSMB<Creature>.Initialise(animator, this);

            // TODO Recommend moving this to player object instead, so that the player can set what colliders it needs to ignore in the game scene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                BoxCollider2D playerBoxCollider = player.GetComponent<BoxCollider2D>();
                CircleCollider2D playerCircleCollider = player.GetComponent<CircleCollider2D>();
                Physics2D.IgnoreCollision(playerBoxCollider, m_Collider);
                Physics2D.IgnoreCollision(playerCircleCollider, m_Collider);
            }

            CurrentHealth = stats.BaseHealth;
            CurrentAgression = stats.BaseAggression;
            Stats = stats;
            gameObject.name = Stats.Name;
            this.attackSet = attackSet;
        }

        protected void UpdateBaseAnimationKeys()
        {
            animator.SetFloat("Speed", Mathf.Abs(m_Rigidbody.velocity.x));
            animator.SetBool("IsGrounded", CheckGrounded());
            animator.SetBool("IsKnockedDown", isTripped || isKnockedOut);
            animator.SetBool("IsStaggered", isStaggered);
            animator.SetBool("IsDead", isDead);
        }

        // Sets the creature state and if it should do a hard override of the current state
        // Hard overrides are for if the state is the same but with new values, since they are structs
        protected abstract Tuple<ICreatureState, bool> DetermineBehavoir();

        public bool CheckGrounded()
        {
            if (isJumping) return false;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, GROUND_RADIUS, GroundLayerMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (!colliders[i].gameObject.Equals(this.gameObject))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void GroundMove(in float move)
        {
            if (!isInAnimationTransition && CheckGrounded() && currentAttack == null && !isKnockedOut && !isTripped && !isStaggered)
            {
                Vector3 targetVelocity = new Vector2(move * Stats.Speed, m_Rigidbody.velocity.y);
                m_Rigidbody.velocity = Vector3.SmoothDamp(m_Rigidbody.velocity, targetVelocity, ref velocity, 0.5f);

                if (move > 0 && !isFacingRight)
                {
                    Flip();
                }
                else if (move < 0 && isFacingRight)
                {
                    Flip();
                }
                if (!hasInitiatedJump && this.jumpEvent != null)
                {
                    hasInitiatedJump = true;
                    animator.SetTrigger("Jump");
                }
            }
        }

        public virtual void Jump()
        {
            if (CheckGrounded() && !isJumping && this.jumpEvent != null)
            {
                StartCoroutine(JumpToDestination(this.jumpEvent.Destination));
                hasInitiatedJump = false;
            }
        }

        private IEnumerator JumpToDestination(Vector2 destination)
        {
            // Finish up attacking first
            if (currentAttack != null) yield return new WaitUntil(() => currentAttack == null); 
            isJumping = true;
            m_Rigidbody.isKinematic = true;
            // Generate new damage ID for downward velocity damage and activate hitboxes
            JUMPING_DOWN_DMG.GenerateNewGuid();
            ActivateHitBoxes(JUMPING_DOWN_DMG);
            // Calculate destination based off of difference between ground check and creature center
            float jumpYHeight = destination.y + Mathf.Abs(transform.position.y - groundCheck.position.y);
            Vector2 end = new Vector2(destination.x, jumpYHeight);
            Vector2 start = transform.position;

            float progress = 0f;
            do
            {
                transform.position = CalculateJumpStep(start, end, jumpYHeight, progress);
                progress += Time.deltaTime / JUMP_DURATION;
                yield return new WaitForEndOfFrame();
            } while (progress < 1f && !IsDead);

            m_Rigidbody.isKinematic = false;
            isJumping = false;
            this.jumpEvent = null;
            ClearActiveHitBoxes();
        }

        /**
         * Used to move creature along a parabola to the jump destion (from start to end based off of provided time and provided peak height of jump)
         */
        private Vector2 CalculateJumpStep(Vector2 start, Vector2 end, float height, float t)
        {
            Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

            var mid = Vector2.Lerp(start, end, t);

            return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
        }

        public void Attack(in CreatureAttack attack)
        {
            if (!isInAnimationTransition && currentAttack == null && !isKnockedOut && !isTripped && !isStaggered)
            {
                // Attack ID of zero is a null catch for creature attacks, no attack IDs should be zero
                if (attack != null)
                {
                    animator.SetInteger("Attack_ID", attack.ID);
                    animator.SetTrigger("Attack");
                    currentAttack = attack;
                    // Done because the damage is only created once for attacks
                    currentAttack.GenerateNewDamageGuid();
                    ActiveAttackFrames = attack.Frames;
                }
            }
        }

        public virtual void ActivateAttackFrame(in int frame)
        {
            CreatureAttackFrame attackFrame;
            // Check if frame is within current active attack frames
            if (ActiveAttackFrames.TryGetValue(frame, out attackFrame))
            {
                // Apply movement from frame
                float movement = isFacingRight ? attackFrame.ForwardMovement : -attackFrame.ForwardMovement;
                m_Rigidbody.AddForce(new Vector2(movement * Stats.Speed, 0), ForceMode2D.Impulse);
                // Activate Hit boxes from frame
                if (attackFrame.ActiveHitboxes?.Length > 0)
                {
                    foreach (Hitbox hitbox in hitboxes)
                    {
                        if (attackFrame.ActiveHitboxes.Contains(hitbox.name) && currentAttack != null)
                        {
                            hitbox.IsActive = true;
                            hitbox.ActiveHitBoxDamage = currentAttack.GetDamage();
                        }
                    }
                }
                else
                {
                    // Clear active hitboxes if no active hitboxes are provided
                    ClearActiveHitBoxes();
                }
                // Swap sprites from frame
                if (attackFrame.SpriteSwaps?.Length > 0)
                {
                    foreach (CreatureAttackSpriteSwap swap in attackFrame.SpriteSwaps)
                    {
                        // Target child bone for swap of creature transform
                        Transform bone = transform.GetComponentsInChildren<Transform>().FirstOrDefault(c => c.gameObject.name == swap.Key);
                        // Get parent of bone because that is where the sprites end up on a skeletal rig
                        SpriteResolver resolver = bone.GetComponentInParent<SpriteResolver>();
                        resolver.SetCategoryAndLabel(swap.Category, swap.Label);
                    }
                }
                // Create effects from frame
                if (!attackFrame.EffectId.Equals(CreatureEffectID.NONE))
                {
                    // Get transform of source of effect from frame
                    Transform source = transform.GetComponentsInChildren<Transform>().FirstOrDefault(c => c.gameObject.name == attackFrame.EffectSourceId);
                    // Spawn effect at source transform position and child it to this creature object
                    GameObject effect = Instantiate(CreatureAttackEffectLoader.LoadEffect(attackFrame.EffectId), this.transform);
                    effect.transform.position = source.position;
                }
            }
        }

        /*
        * Activates all the hitboxes with the provided damage, typically used for a creature jumping or swooping
        */
        private void ActivateHitBoxes(in Damage damage)
        {
            for (int i = 0; i < hitboxes.Length; i++)
            {
                hitboxes[i].IsActive = true;
                hitboxes[i].ActiveHitBoxDamage = damage;
            }
        }

        public void EndAttack()
        {
            currentAttack = null;
            ClearActiveHitBoxes();
        }

        public virtual void Damage(in Damage dmg, in CreaturePartDamageModifier dmgMod = CreaturePartDamageModifier.NONE, in float dmgModAmount = 1, bool forceKnockout = false, bool forceTrip = false)
        {
            float calculatedDmg = dmg.Value * dmgModAmount;
            // Calculate damage based off of resisted types
            float resistMod;
            if (Stats.ResistedElements.TryGetValue(dmg.Type, out resistMod))
            {
                // Calculate the difference of the resistance mod, if it is a negative resistance mod (a weakness); add that damage to the calculated damage
                float calculatedResistDmg = calculatedDmg - (calculatedDmg / Mathf.Abs(resistMod));
                if (resistMod < 0) calculatedResistDmg *= -1;
                calculatedDmg -= calculatedResistDmg;
            }
            // Calculate affected tripping threshold
            if (dmgMod.Equals(CreaturePartDamageModifier.TRIP) && !isTripped || forceTrip)
            {
                // Apply chop modifier to trip damage
                float tripDmg = dmg.Mods[DamageModType.CHOP] * calculatedDmg;
                CurrentTripThreshold += (GetCripplePercent(CreaturePartsType.Ground) >= .5f || GetCripplePercent(CreaturePartsType.Flight) >= .5f) ? (tripDmg * 1.5f) : tripDmg;
                if (CurrentTripThreshold >= Stats.TripThreshold || forceTrip)
                {
                    CurrentTripThreshold = 0;
                    isStaggered = false;
                    isTripped = true;
                    EndAttack();
                    // Don't restart down count if already knocked out
                    if (!isKnockedOut)
                    {
                        StartCoroutine(StartGetUpTimer(TRIPPED_DOWN_TIME));
                    }
                }
            }
            // Calculate affected knock out threshold
            if (dmgMod.Equals(CreaturePartDamageModifier.KO) && !isKnockedOut || forceKnockout)
            {
                // Apply strike modifier to knock out damage
                float koDmg = dmg.Mods[DamageModType.STRIKE] * calculatedDmg;
                // Increase knockout damage modifier if mobility part cripple percentage is high enough
                CurrentKOThreshold += (GetCripplePercent(CreaturePartsType.Ground) >= .5f || GetCripplePercent(CreaturePartsType.Flight) >= .5f) ? (koDmg * 1.5f) : koDmg;
                if (CurrentKOThreshold >= Stats.KOThreshold || forceKnockout)
                {
                    CurrentKOThreshold = 0;
                    isStaggered = false;
                    isKnockedOut = true;
                    EndAttack();
                    // Don't restart down count if already tripped
                    if (!isTripped)
                    {
                        StartCoroutine(StartGetUpTimer(KNOCK_OUT_DOWN_TIME));
                    }
                }
            }

            if (CurrentKOThreshold >= (Stats.KOThreshold / 2) && CurrentTripThreshold >= (Stats.TripThreshold / 2) && !IsStaggered && !isKnockedOut && !isTripped && UnityEngine.Random.Range(0, 100) <= STAGGER_CHANCE)
            {
                isStaggered = true;
                // Recover some KO and Trip Threshold to prevent stagger locking patterns
                CurrentKOThreshold -= (CurrentKOThreshold / 1.5f);
                CurrentTripThreshold -= (CurrentTripThreshold / 1.5f);
                StartCoroutine(StartStaggerTimer());
            }

            CurrentHealth -= calculatedDmg;

            if (CurrentHealth <= 0)
            {
                isStaggered = false;
                isKnockedOut = false;
                isTripped = false;
                EndAttack();
                isDead = true;
            }
        }

        private IEnumerator StartStaggerTimer()
        {
            yield return new WaitForSeconds(STAGGER_TIME);
            isStaggered = false;
        }

        private IEnumerator StartGetUpTimer(float getUpTime)
        {
            yield return new WaitForSeconds(getUpTime);
            isKnockedOut = false;
            isTripped = false;
        }

        public void Flinch()
        {
            EndAttack();
            animator.SetTrigger("Flinch");
        }

        public void SpawnEffectOnCreature(Vector3 sourceOfEffect, CreatureOnEffect fx)
        {
            Transform effectSource = null;
            float closestDistanceSqr = Mathf.Infinity;
            foreach (Hitbox potentialTarget in hitboxes)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - sourceOfEffect;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    effectSource = potentialTarget.transform;
                }
            }

            GameObject effect = null;
            ParticleSystem.MainModule settings;
            switch (fx)
            {
                case CreatureOnEffect.BloodSplash:
                    effect = EffectsManager.Instance.BloodSplashLarge;
                    break;
                case CreatureOnEffect.BloodSpurt:
                    effect = EffectsManager.Instance.BloodSpurt;
                    break;
            }
            if (effect != null)
            {
                settings = effect.GetComponent<ParticleSystem>().main;
                settings.startColor = BloodColor;
                Instantiate(effect, effectSource.position, effectSource.rotation, effectSource);
            }
        }

        /**
         * Grab the percentage the creature is crippled based off of the provided parts type
         */
        public float GetCripplePercent(in CreaturePartsType type)
        {
            CreaturePart[] parts = type.Equals(CreaturePartsType.Ground) ? GroundMobilityParts : FlightMobilityParts;
            int totalMobileParts = parts.Length;

            int brokenPartCount = 0;
            foreach (CreaturePart part in parts)
            {
                if (part.IsBroken) brokenPartCount++;
            }
            return (float)(totalMobileParts - brokenPartCount) / totalMobileParts;
        }

        private void ClearActiveHitBoxes()
        {
            foreach (Hitbox hitbox in hitboxes)
            {
                hitbox.IsActive = false;
                hitbox.ActiveHitBoxDamage = null;
            }
        }


        protected void Flip()
        {
            isFacingRight = !isFacingRight;

            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }


        public bool IsFacingRight
        {
            get { return isFacingRight; }
        }

        public bool IsStaggered
        {
            get { return isStaggered; }
        }

        public bool IsDead
        {
            get { return isDead; }
        }

        public CreatureAttack[] AttackSet
        {
            get { return attackSet; }
        }

        public CreatureAiStateMachine AiStateMachine
        {
            get { return aiStateMachine; }
        }

        public Transform GroundCheck
        {
            get { return groundCheck; }
        }
    }
}
