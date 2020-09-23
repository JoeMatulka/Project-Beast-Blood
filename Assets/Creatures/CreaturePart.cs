using HitboxSystem;
using UnityEngine;
/**
* Class meant to represent a body part on the creature that registers damage on hit, can be breakable
*/
public class CreaturePart : MonoBehaviour
{
    public float PartHealth = 1000;

    public bool IsBreakable;

    public Hitbox[] hitBoxes;

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
                    hitbox.Collider.isTrigger = true;
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