using CreatureSystems;
using UnityEngine;

public class CreatureSearchForTargetBehavior : ICreatureState
{
    private const float WALK_INPUT = .5f;

    private const float TURN_RANGE = 2f;

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
        Vector2 creaturePos = creature.transform.localPosition;

        // Try to find a target, if found return because we don't want to continue
        Transform target = FindTarget(creaturePos);
        if (target != null) return;

        SeekTarget(creaturePos);
    }

    private Transform FindTarget(in Vector2 creaturePos)
    {
        Transform target = null;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(creaturePos, sightRange, sightLayerMask);
        foreach (Collider2D col in hitColliders)
        {
            // Ignore collision with source creature
            if (!col.gameObject.Equals(creature.gameObject))
            {
                // Check to see if target is in line of sight
                Vector2 targetPos = col.gameObject.transform.localPosition;
                RaycastHit2D hit = Physics2D.Raycast(creaturePos, targetPos - creaturePos, sightRange, sightLayerMask);
                // TODO Prefer player for now, need to account for creature aggressiveness towards other things
                if (hit.collider != null && hit.collider.transform.tag.Equals("Player"))
                {
                    Debug.DrawRay(creaturePos, targetPos - creaturePos, Color.green);
                    creature.Target = col.transform;
                    target = creature.Target;
                    break;
                }
                else
                {
                    Debug.DrawRay(creaturePos, targetPos - creaturePos, Color.red);
                }
            }
        }
        return target;
    }

    private void SeekTarget(in Vector2 creaturePos)
    {
        float movement = 0f;
        bool jump = false;

        if (lastPositionOfTarget != null)
        {
            // Seek aimlessly if no previous position of target has been set
            bool isFacingRight = creature.IsFacingRight;
            movement = isFacingRight ? WALK_INPUT : -WALK_INPUT;
            // TODO Turn when detect collision with a path blocker
        }

        creature.GroundMove(movement, jump);
    }

    public void Exit()
    {

    }
}
