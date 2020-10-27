using UnityEngine;

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

    protected void Move(float move, bool jump)
    {
        if (CheckGrounded())
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


    protected void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}


