using Gamekit2D;
using HitboxSystem;
using CreatuePartSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Animator))]
    public abstract class Creature : MonoBehaviour
    {
        public struct CreatureStats
        {
            public float BaseHealth;
            public float TripThreshold;
            public float KOThreshold;
            public float Speed;
            public float JumpForce;
            // Not meant to be the actual size in game, just representative of size in the fantasy term (in feet).
            public float BaseSize;
            public float SizeModifier;
            public CreatureType CreatureType;
            // Resisted element and the resistance value to it
            public Dictionary<DamageType, float> ResistedElements;
        }

        public Creature.CreatureStats Stats;

        [SerializeField]
        protected float CurrentHealth;
        [SerializeField]
        protected float CurrentTripThreshold = 0;
        [SerializeField]
        protected float CurrentKOThreshold = 0;
        [SerializeField]
        protected CreaturePart[] GroundMobilityParts;
        [SerializeField]
        protected CreaturePart[] FlightMobilityParts;
        [SerializeField]
        protected CreaturePart[] AttackParts;
        [SerializeField]
        private LayerMask WhatIsGround;
        [SerializeField]
        private Transform groundCheck;

        private readonly float GROUND_RADIUS = 1f;

        private Vector3 velocity = Vector3.zero;

        private bool isFacingRight = false;

        protected const float WALK_INPUT = 1f;
        protected const float RUN_INPUT = 2f;

        protected Rigidbody2D m_Rigidbody;
        protected CircleCollider2D m_Collider;

        protected Animator animator;

        public Transform Target;
        private Hitbox[] hitboxes;
        private CreatureAttack currentAttack;
        private Dictionary<int, CreatureAttackFrame> ActiveAttackFrames = new Dictionary<int, CreatureAttackFrame>();

        /**
         * Should be called in Awake phase of a creature object 
         **/
        protected void InitialSetUp(Creature.CreatureStats stats)
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
            Stats = stats;
            // This sets the main creature object to ignore raycasts, this is because hit detection for a creature should happen at the creature part > hitbox level. Not at the highest parent object, being the creature object
            this.gameObject.layer = 2;
        }

        protected void UpdateBaseAnimationKeys()
        {
            animator.SetFloat("Speed", Mathf.Abs(m_Rigidbody.velocity.x));
            animator.SetBool("IsGrounded", CheckGrounded());
        }

        protected bool CheckGrounded()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, GROUND_RADIUS, WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void GroundMove(float move, bool jump)
        {
            if (CheckGrounded() && currentAttack == null)
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
            }
            if (CheckGrounded() && jump)
            {
                m_Rigidbody.AddForce(new Vector2(0f, Stats.JumpForce));
            }
        }

        protected virtual void Attack()
        {
            if (currentAttack == null)
            {
                CreatureAttack attack = CreatureAttackBehavior.CalculateAttack(Target.position, this);
                // Attack ID of zero is a null catch for creature attacks, no attack IDs should be zero
                if (attack != null)
                {
                    animator.SetInteger("Attack_ID", attack.ID);
                    animator.SetTrigger("Attack");
                    currentAttack = attack;
                    ActiveAttackFrames = attack.Frames;
                }
            }
        }

        public virtual void ActivateAttackFrame(int frame)
        {
            CreatureAttackFrame attackFrame;
            if (ActiveAttackFrames.TryGetValue(frame, out attackFrame))
            {
                // Apply movement from frame
                float movement = isFacingRight ? attackFrame.ForwardMovement : -attackFrame.ForwardMovement;
                m_Rigidbody.AddForce(new Vector2(movement, 0), ForceMode2D.Impulse);
                // Activate Hit boxes from frame
                if (attackFrame.ActiveHitboxes?.Length > 0)
                {
                    foreach (Hitbox hitbox in hitboxes)
                    {
                        if (attackFrame.ActiveHitboxes.Contains(hitbox.name))
                        {
                            hitbox.IsActive = true;
                            hitbox.ActiveHitBoxDamage = currentAttack.Damage;
                        }
                    }
                }
                else
                {
                    // Clear active hitboxes
                    ClearActiveHitBoxes();
                }
            }
        }

        public void EndAttack()
        {
            currentAttack = null;
        }

        public virtual void Damage(in Damage dmg, in CreaturePartDamageModifier dmgMod = CreaturePartDamageModifier.NONE, in float dmgModAmount = 1)
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
            if (dmgMod.Equals(CreaturePartDamageModifier.TRIP))
            {
                CurrentTripThreshold += calculatedDmg;
                if (CurrentTripThreshold >= Stats.TripThreshold)
                {
                    CurrentTripThreshold = 0;
                    // TODO Set Creature State to tripped here
                }
            }
            // Calculate affected knock out threshold
            if (dmg.Equals(CreaturePartDamageModifier.KO))
            {
                // Increase knockout damage modifier if mobility part cripple percentage is high enough
                CurrentKOThreshold += (GetCripplePercent(CreaturePartsType.Ground) >= .5f || GetCripplePercent(CreaturePartsType.Flight) >= .5f) ? (calculatedDmg * 1.5f) : calculatedDmg;
                if (CurrentKOThreshold >= Stats.KOThreshold)
                {
                    CurrentKOThreshold = 0;
                    // TODO Set Creature State to knocked out here
                }
            }

            CurrentHealth -= calculatedDmg;
        }

        /**
         * Grab the percentage the creature is crippled based off of the provided parts type
         */
        public float GetCripplePercent(CreaturePartsType type)
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
    }
}
