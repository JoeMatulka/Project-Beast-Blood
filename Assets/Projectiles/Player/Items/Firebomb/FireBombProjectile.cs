using HitboxSystem;
using ResourceManager;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FireBombProjectile : MonoBehaviour
{
    private BoxCollider2D col;
    private Rigidbody2D rb;

    private readonly Damage CONTACT_DAMAGE = new Damage(5, DamageType.RAW);

    private readonly Damage EXPLOSION_DAMAGE = new Damage(2, DamageType.FIRE);
    private const float EXPLOSION_SIZE = .5f;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Damage Other on Contact if it has a hit box
        Hitbox hb = collision.transform.GetComponent<Hitbox>();
        if (hb != null) hb.ReceiveDamage(CONTACT_DAMAGE, this.transform.position);
        // Spawn explosion on contact
        GameObject explosionGO = Instantiate(EffectsManager.Instance.FireExplosion);
        explosionGO.transform.position = this.transform.position;
        Explosion explosion = explosionGO.GetComponent<Explosion>();
        if (explosion != null)
        {
            explosion.Damage = EXPLOSION_DAMAGE;
            explosion.Size = EXPLOSION_SIZE;
        }
        // Spawn flames that stay and burn

        // Destroy this game object
        Destroy(this.gameObject);
    }
}
