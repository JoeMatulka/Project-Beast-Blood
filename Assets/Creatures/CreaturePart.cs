using HitboxSystem;
using UnityEngine;
/**
* Class meant to represent a body part on the creature that registers damage on hit, can be breakable
*/
public class CreaturePart : MonoBehaviour
{
    [SerializeField]
    public float PartHealth = 1000;

    public bool IsBreakable;

    public Hitbox[] hitBoxes;

    // Meant to allow certain parts to act as triggers to allow the player to easily navigate around the hit box
    public bool IsHitBoxTrigger = false;

    private bool isBroken = false;

    private void Start()
    {
        if (hitBoxes != null && hitBoxes.Length > 0)
        {
            foreach (Hitbox hitbox in hitBoxes)
            {
                if (hitbox != null)
                {
                    hitbox.Handler += new Hitbox.HitboxEventHandler(OnHit);
                    hitbox.Collider.isTrigger = IsHitBoxTrigger;
                }
                else
                {
                    Debug.LogError(gameObject.name + " tried to set a hit box on limb that wasn't set");
                }
            }
        }
        else
        {
            Debug.LogError(gameObject.name + " creature part was set without a hit box");
        }
    }

    private void OnHit(object sender, HitboxEventArgs e)
    {
        Debug.Log(gameObject.name + " was hit");
    }
}