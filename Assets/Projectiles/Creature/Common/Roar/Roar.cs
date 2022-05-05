using CreatureSystems;
using HitboxSystem;
using UnityEngine;

public class Roar : MonoBehaviour
{
    private Transform source;

    private const float ROAR_RADIUS = 7.5f;

    private const float ROAR_LIFE = 1f;

    private readonly Damage ROAR_DMG = new Damage(0, DamageElementType.RAW, new Vector2(5, 0));

    private LayerMask roarMask;

    void Awake()
    {
        roarMask = LayerMask.GetMask("Ground", "Ignore Raycast", "Creature Jump Trigger");
        source = this.transform.parent;
    }

    void Start()
    {
        Destroy(this.gameObject, ROAR_LIFE);
    }

    void LateUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, ROAR_RADIUS, ~roarMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            // Ignore collisions with child game objects of source object, mainly for creatures
            if (!colliders[i].transform.IsChildOf(source))
            {
                // Only apply damage to things that have hit boxes
                Hitbox hitbox = colliders[i].GetComponent<Hitbox>();
                if (hitbox != null)
                {
                    hitbox.ReceiveDamage(ROAR_DMG, this.transform.position);
                }
            }
        }

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, ROAR_RADIUS);
    }
}
