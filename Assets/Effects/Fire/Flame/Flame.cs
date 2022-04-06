using HitboxSystem;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Flame : MonoBehaviour
{
    public float Lifetime = 5f;

    public Damage Damage = new Damage(1, DamageType.FIRE);

    private const float REDUCE_TIME = 5f;
    private const float REDUCE_STEP = .01f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Invoke("ReduceFlame", Lifetime);
    }

    private void ReduceFlame()
    {
        StartCoroutine(ReduceSize());
    }

    private IEnumerator ReduceSize()
    {
        float reduceTime = 0;
        while (reduceTime <= REDUCE_TIME)
        {
            reduceTime += REDUCE_STEP;
            transform.localScale = new Vector3(transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z / 2);
            yield return new WaitForFixedUpdate();
        }
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Stop the flame if it is falling, delay the collision to ensure the flame is set onto the parent
        Invoke("SetKinematic", .35f);
        // Make flame part of transform it collided with
        this.transform.parent = collision.transform;
        // Apply fire damage if collision has a hit box
        ApplyDamageToHitbox(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ApplyDamageToHitbox(collision);
    }

    private void ApplyDamageToHitbox(Collider2D collision)
    {
        Hitbox hitbox = collision.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.ReceiveDamage(Damage, this.transform.localPosition);
        }
    }

    private void SetKinematic() { rb.isKinematic = true; }
}
