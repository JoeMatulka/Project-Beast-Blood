using HitboxSystem;
using ResourceManager;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Flame : MonoBehaviour
{
    public float Lifetime = 5f;

    public Damage Damage = new Damage(1, DamageType.FIRE);
    // How often fire damage is applied
    private const float DAMAGE_STEP = .35f;

    private const float REDUCE_TIME = 5f;
    private const float REDUCE_STEP = .01f;
    private bool reducing = false;

    private Rigidbody2D rb;

    private const float COL_SIZE = .175f;
    private const float DELAY_COL_TIME = 1f;

    private LayerMask collisonMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        collisonMask = LayerMask.GetMask("Creature Jump Trigger", "Particle", "Ignore Raycast", "Creature");
    }

    void Start()
    {
        Invoke("StartCheckingCollisions", DELAY_COL_TIME);
        Invoke("ReduceFlame", Lifetime);
    }

    private void ReduceFlame()
    {
        StartCoroutine(ReduceSize());
    }

    private void StartCheckingCollisions()
    {
        StartCoroutine(CheckCollisions());
    }

    private IEnumerator CheckCollisions()
    {
        while (!reducing)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, COL_SIZE, ~collisonMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                // Ignore collision with other flame objects
                if (colliders[i].GetComponent<Flame>() == null)
                {
                    // Apply to parent if applicable
                    this.transform.parent = colliders[i].transform;
                    // Apply kinematic and freeze positon
                    rb.isKinematic = true;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    // Only apply damage to things that have hit boxes
                    Hitbox hitbox = colliders[i].GetComponent<Hitbox>();
                    if (hitbox != null)
                    {
                        hitbox.ReceiveDamage(Damage, this.transform.position);
                        yield return new WaitForSeconds(DAMAGE_STEP);
                    }
                }

            }
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ReduceSize()
    {
        Instantiate(EffectsManager.Instance.Smolder, this.transform.position, this.transform.rotation);
        reducing = true;
        float reduceTime = 0;
        while (reduceTime <= REDUCE_TIME)
        {
            reduceTime += REDUCE_STEP;
            transform.localScale = new Vector3(transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z / 2);
            yield return new WaitForFixedUpdate();
        }
        Destroy(this.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, COL_SIZE);
    }
}
