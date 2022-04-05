using HitboxSystem;
using System.Collections;
using UnityEngine;

/**
 * It's important to note that this object is destroyed by the particle system cleaner, not the greatest solution but works for now
 * */
public class Explosion : MonoBehaviour
{
    public float Size;
    private float currentSize = 0;
    private const float SIZE_STEP = .25f;

    public Damage Damage = new Damage(0, DamageType.RAW);

    private LayerMask explosionMask;

    void Awake()
    {
        explosionMask = LayerMask.GetMask("Ground", "Ignore Raycast", "Creature Jump Trigger");
    }

    void Start()
    {
        StartCoroutine(GrowExplosion());
    }

    private IEnumerator GrowExplosion()
    {
        while (currentSize <= Size)
        {
            currentSize += SIZE_STEP;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, currentSize, ~explosionMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                // Only apply damage to things that have hit boxes
                Hitbox hitbox = colliders[i].GetComponent<Hitbox>();
                if (hitbox != null)
                {
                    hitbox.ReceiveDamage(Damage, this.transform.position);
                }

            }
            yield return new WaitForFixedUpdate();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, currentSize);
    }
}
