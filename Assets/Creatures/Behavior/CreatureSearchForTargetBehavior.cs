using CreatureSystems;
using UnityEngine;

public class CreatureSearchForTargetBehavior : ICreatureState
{
    private readonly Creature creature;
    private readonly Vector2 lastPositionOfTarget;
    private readonly float sightRange;
    private readonly LayerMask sightLayerMask;

    public CreatureSearchForTargetBehavior(Creature creature, float sightRange, LayerMask sightLayerMask)
    {
        this.creature = creature;
        this.sightRange = sightRange;
        this.sightLayerMask = sightLayerMask;
    }

    public CreatureSearchForTargetBehavior(Creature creature, Vector2 lastPositionOfTarget, float sightRange, LayerMask sightLayerMask)
    {
        this.creature = creature;
        this.lastPositionOfTarget = lastPositionOfTarget;
        this.sightRange = sightRange;
        this.sightLayerMask = sightLayerMask;
    }

    public void Enter()
    {

    }

    public void Execute()
    {
        // TODO Add seek out movement here based off of if there is a last position of target

        Vector2 creaturePos = creature.transform.localPosition;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(creaturePos, sightRange, sightLayerMask);
        foreach (Collider2D col in hitColliders)
        {
            // Ignore collision with source creature
            if (!col.gameObject.Equals(creature.gameObject))
            {
                // Check to see if target is in line of sight
                Vector2 targetPos = col.gameObject.transform.localPosition;
                RaycastHit2D hit = Physics2D.Raycast(creaturePos, targetPos - creaturePos, sightRange, sightLayerMask);
                if (hit.collider != null && hit.collider.transform.tag.Equals("Player"))
                {
                    Debug.DrawRay(creaturePos, targetPos - creaturePos, Color.green);
                    creature.Target = col.transform;
                }
                else
                {
                    Debug.DrawRay(creaturePos, targetPos - creaturePos, Color.red);
                }
            }
        }
    }

    public void Exit()
    {

    }
}
