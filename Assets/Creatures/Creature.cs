﻿using Gamekit2D;
using System.Collections.Generic;
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
    protected CreaturePart[] Parts;
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
    private bool isAttacking = false;
    public Dictionary<int, CreatureAttackFrame> ActiveAttackFrames = new Dictionary<int, CreatureAttackFrame>();

    /**
     * Should be called in Awake phase of a creature object 
     **/
    protected void InitialSetUp(float health, float speed, float jumpForce, float attackRange)
    {
        Parts = this.GetComponentsInChildren<CreaturePart>();
        m_Rigidbody = this.GetComponent<Rigidbody2D>();
        m_Rigidbody.freezeRotation = true;
        m_Collider = this.GetComponent<CircleCollider2D>();
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
        Debug.Log("Next time you code, fix the assigning of ActiveAttackFrames to the creature. They seem to be set and called once and then never again...");
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
        if (CheckGrounded() && !isAttacking)
        {
            Vector3 targetVelocity = new Vector2(move * Speed, m_Rigidbody.velocity.y);

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

    protected virtual void Attack()
    {
        animator.SetInteger("Attack_ID", CreatureAttackBehavior.GetAttack(Target.position, this));
        animator.SetTrigger("Attack");
        this.isAttacking = true;
    }

    public virtual void ActivateAttackFrame(int frame)
    {
        CreatureAttackFrame attackFrame;
        if (ActiveAttackFrames.TryGetValue(frame, out attackFrame)) {
            Debug.Log(frame);
        }
    }

    public void EndAttack()
    {
        this.isAttacking = false;
        this.ActiveAttackFrames.Clear();
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


