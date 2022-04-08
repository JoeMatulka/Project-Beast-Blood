using HitboxSystem;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Flame : MonoBehaviour
{
    public float Lifetime = 5f;

    public Damage Damage = new Damage(5, DamageType.FIRE);

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
        // Become kinematic with contact with ground
        if (collision.gameObject.layer.Equals("Ground")) rb.isKinematic = true;
        // Apply fire damage if collision has a hit box
        Hitbox hb = collision.GetComponent<Hitbox>();
        if (hb != null)
        {
            ApplyDamageToHitbox(hb);
        }
    }

    private void ApplyDamageToHitbox(in Hitbox hb)
    {
        Hitbox hitbox = hb.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.ReceiveDamage(Damage, this.transform.localPosition);
        }
    }
}
