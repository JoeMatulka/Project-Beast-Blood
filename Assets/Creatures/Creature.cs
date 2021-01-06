using Gamekit2D;
using HitboxSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CreatureType
{
    Bipedal
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Animator))]
public abstract class Creature : MonoBehaviour
{
    [SerializeField]
    protected float Health;
    [SerializeField]
    protected float Size;
    [SerializeField]
    protected CreaturePart[] MobilityParts;
    [SerializeField]
    protected CreaturePart[] AttackParts;
    [SerializeField]
    protected float JumpForce;
    [SerializeField]
    protected float Speed;
    [SerializeField]
    protected CreatureType m_Type;
    [SerializeField]
    private LayerMask WhatIsGround;
    [SerializeField]
    private Transform groundCheck;

    private readonly float GROUND_RADIUS = 1f;

    private Vector3 velocity = Vector3.zero;

    private bool isFacingRight = false;

    protected const float WALK_INPUT = 0.25f;
    protected const float RUN_INPUT = 1f;

    protected Rigidbody2D m_Rigidbody;
    protected CircleCollider2D m_Collider;

    protected Animator animator;

    public Transform Target;
    protected float attackRange;
    private Hitbox[] hitboxes;
    private CreatureAttack currentAttack;
    private Dictionary<int, CreatureAttackFrame> ActiveAttackFrames = new Dictionary<int, CreatureAttackFrame>();

    /**
     * Should be called in Awake phase of a creature object 
     **/
    protected void InitialSetUp(float health, float speed, float jumpForce, float attackRange)
    {
        m_Rigidbody = this.GetComponent<Rigidbody2D>();
        m_Rigidbody.freezeRotation = true;
        m_Collider = this.GetComponent<CircleCollider2D>();
        hitboxes = this.GetComponentsInChildren<Hitbox>();
        animator = this.GetComponent<Animator>();
        SceneLinkedSMB<Creature>.Initialise(animator, this);
        // TODO Recommend moving this to player object instead
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            BoxCollider2D playerBoxCollider = player.GetComponent<BoxCollider2D>();
            CircleCollider2D playerCircleCollider = player.GetComponent<CircleCollider2D>();
            Physics2D.IgnoreCollision(playerBoxCollider, m_Collider);
            Physics2D.IgnoreCollision(playerCircleCollider, m_Collider);
        }
        Health = health;
        Speed = speed;
        JumpForce = jumpForce;
        this.attackRange = attackRange;
        // This sets the main creature object to ignore raycasts, this is because hit detection for a creature should happen at the creature part > hitbox level. Not at the highest parent object, being the creature object
        this.gameObject.layer = 2;
        Debug.Log("Fix conditionally applying movement based on broken parts");
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

    protected virtual void Move(float move, bool jump)
    {
        if (CheckGrounded() && currentAttack == null)
        {
            float calcMove = calculateMoveFromMobilityParts(move);
            Vector3 targetVelocity = new Vector2(calcMove * Speed, m_Rigidbody.velocity.y);
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
            m_Rigidbody.AddForce(new Vector2(0f, JumpForce));
        }
    }

    private float calculateMoveFromMobilityParts(float move) {
        int totalMobileParts = MobilityParts.Length;
        int brokenPartCount = 0;
        float calcMove = move;
        foreach (CreaturePart part in MobilityParts) {
            if (part.IsBroken) brokenPartCount++;
        }
        // If half parts are broken, can only walk. If all parts are broken; can't move
        if ((totalMobileParts / 2) <= brokenPartCount) {
            calcMove = move / 2;
        } else if (totalMobileParts == brokenPartCount) {
            calcMove = 0;
        }
        return calcMove;
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

    public virtual void Damage(float dmg)
    {
        // TODO Do damage mitigation based off of creature stats
        Health -= dmg;
    }

    private void ClearActiveHitBoxes()
    {
        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.IsActive = false;
            hitbox.ActiveHitBoxDamage = null;
        }
    }

    public void EndAttack()
    {
        currentAttack = null;
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

    public CreatureType Type
    {
        get { return m_Type; }
    }
}


