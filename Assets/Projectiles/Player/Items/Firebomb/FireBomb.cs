using HitboxSystem;
using ResourceManager;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FireBomb : MonoBehaviour
{
    private BoxCollider2D col;
    private Rigidbody2D rb;

    private readonly Damage CONTACT_DAMAGE = new Damage(5, DamageType.RAW);

    private readonly Damage EXPLOSION_DAMAGE = new Damage(1, DamageType.FIRE);
    private const float EXPLOSION_SIZE = .5f;

    private const float AMOUNT_FLAMES_SPAWN = 8;
    private const float FLAME_SPAWN_STEP = .25f;
    
    private const float FLAMES_SPAWN_THRESHOLD = FLAME_SPAWN_STEP * AMOUNT_FLAMES_SPAWN;
    private const float FLAME_SPAWN_FORCE = .4f;
    private const float FLAME_LIFETME = 7.5f;

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
        // Spawn flames from explosion in different directions
        for (float i = 0; i <= FLAMES_SPAWN_THRESHOLD; i += FLAME_SPAWN_STEP)
        {
            GameObject flame = Instantiate(ProjectileMananger.Instance.Flame, new Vector2(this.transform.position.x, this.transform.position.y + .5f), this.transform.rotation);
            flame.GetComponent<Flame>().Lifetime = FLAME_LIFETME;
            flame.GetComponent<Rigidbody2D>().AddForce(new Vector2(
                Mathf.Clamp(Vector2.left.x + i, -1, 1),
                Mathf.Clamp(Vector2.left.y + (i + 5), -1, 1)
                ) * FLAME_SPAWN_FORCE, ForceMode2D.Impulse);
        }
        // Destroy this game object
        Destroy(this.gameObject);
    }
}
