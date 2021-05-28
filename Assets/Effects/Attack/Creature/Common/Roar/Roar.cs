using CreatureSystems;
using HitboxSystem;
using UnityEngine;

public class Roar : MonoBehaviour
{
    private Creature creature;

    private const float ROAR_RADIUS = 7.5f;

    private const float ROAR_LIFE = 1f;

    private readonly Damage ROAR_DMG = new Damage(0, DamageType.RAW, new Vector2(5, 0));

    private LayerMask roarMask;

    void Awake()
    {
        creature = GetComponentInParent<Creature>();
        roarMask = LayerMask.GetMask("Creature", "Ground", "Ignore Raycast", "Creature Jump Trigger");
    }

    void Start()
    {
        Destroy(this, ROAR_LIFE);
    }

    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, ROAR_RADIUS, ~roarMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            // Ignore collisions with child game objects of creature object
            if (!colliders[i].transform.IsChildOf(creature.transform))
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
        Gizmos.DrawWireSphere(this.transform.localPosition, ROAR_RADIUS);
    }
}
