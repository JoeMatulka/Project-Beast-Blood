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
    private LayerMask WhatIsGround;
    [SerializeField]
    private Transform groundCheck;

    private readonly float GROUND_RADIUS = 0.5f;

    private Vector3 velocity = Vector3.zero;

    private bool isFacingRight = false;

    protected Rigidbody2D m_Rigidbody;

    protected CircleCollider2D m_Collider;

    protected void InitialSetUp()
    {
        Parts = this.GetComponentsInChildren<CreaturePart>();
        m_Rigidbody = this.GetComponent<Rigidbody2D>();
        m_Rigidbody.freezeRotation = true;
        m_Collider = this.GetComponent<CircleCollider2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            BoxCollider2D playerBoxCollider = player.GetComponent<BoxCollider2D>();
            CircleCollider2D playerCircleCollider = player.GetComponent<CircleCollider2D>();
            Physics2D.IgnoreCollision(playerBoxCollider, m_Collider);
            Physics2D.IgnoreCollision(playerCircleCollider, m_Collider);
        }
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

    public void Move(float move, bool crouch, bool jump)
    {
        //only control the player if grounded or airControl is turned on
        if (CheckGrounded())
        {
            Vector3 targetVelocity = new Vector2(0, m_Rigidbody.velocity.y);

            // And then smoothing it out and applying it to the character
            m_Rigidbody.velocity = Vector3.SmoothDamp(m_Rigidbody.velocity, targetVelocity, ref velocity, 0.5f);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !isFacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && isFacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        // If the player should jump...
        if (CheckGrounded() && jump)
        {
            // Add a vertical force to the player.
            m_Rigidbody.AddForce(new Vector2(0f, JumpForce));
        }
    }


    public void Flip()
    {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}


