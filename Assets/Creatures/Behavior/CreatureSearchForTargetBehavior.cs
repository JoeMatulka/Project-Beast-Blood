using CreatureSystems;
using UnityEngine;

public struct CreatureSearchForTargetBehavior : ICreatureState
{
    private readonly Creature creature;
    private Vector2 lastPositionOfTarget;
    private readonly float sightRange;
    private readonly float collisionRange;
    private readonly LayerMask sightLayerMask;
    private readonly LayerMask groundLayerMask;
    private readonly bool slowed;

    public CreatureSearchForTargetBehavior(Creature creature, float sightRange, float collisionRange, LayerMask sightLayerMask, bool slowed = false)
    {
        this.creature = creature;
        this.sightRange = sightRange;
        this.collisionRange = collisionRange;
        this.sightLayerMask = sightLayerMask;
        this.groundLayerMask = LayerMask.GetMask("Ground");
        this.lastPositionOfTarget = Vector2.zero;
        this.slowed = slowed;
    }

    public CreatureSearchForTargetBehavior(Creature creature, Vector2 lastPositionOfTarget, float sightRange, float collisionRange, LayerMask sightLayerMask, bool slowed = false)
    {
        this.creature = creature;
        this.lastPositionOfTarget = lastPositionOfTarget;
        this.sightRange = sightRange;
        this.collisionRange = collisionRange;
        this.sightLayerMask = sightLayerMask;
        this.groundLayerMask = LayerMask.GetMask("Ground");
        this.slowed = slowed;
    }

    public void Enter() {
        if (lastPositionOfTarget != Vector2.zero) {
            // Draw Debug Reference for where the creature is going to seek
            Debug.DrawRay(lastPositionOfTarget, Vector2.up * 1, Color.green);
        }
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(creaturePos, sightRange, sightLayerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            // Ignore collision with source creature
            if (!colliders[i].gameObject.Equals(creature.gameObject))
            {
                // Check to see if target is in line of sight
                Vector2 targetPos = colliders[i].gameObject.transform.localPosition;
                RaycastHit2D[] hits = Physics2D.RaycastAll(creaturePos, targetPos - creaturePos, sightRange, sightLayerMask);
                for (int ii = 0; ii < hits.Length; ii++)
                {
                    // TODO Prefer player for now, need to account for creature aggressiveness towards other things (creatures, sounds, traps, etc.)
                    if (hits[ii].collider != null && hits[ii].collider.transform.tag.Equals("Player"))
                    {
                        Debug.DrawRay(creaturePos, targetPos - creaturePos, Color.green);
                        creature.Target = hits[ii].collider.transform;
                        target = creature.Target;
                        break;
                    }
                    else
                    {
                        Debug.DrawRay(creaturePos, targetPos - creaturePos, Color.red);
                    }
                }
            }
        }
        return target;
    }

    private void SeekTarget(in Vector2 creaturePos)
    {
        float movement = 0f;
        if (lastPositionOfTarget == Vector2.zero || Vector2.Distance(lastPositionOfTarget, creaturePos) <= collisionRange)
        {
            // Seek aimlessly if no previous position of target has been set
            bool isFacingRight = creature.IsFacingRight;
            movement = isFacingRight ? Creature.WALK_INPUT : -Creature.WALK_INPUT;
            Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(creaturePos, dir, collisionRange, groundLayerMask);
            if (hit.collider != null)
            {
                Debug.DrawRay(creaturePos, dir * collisionRange, Color.green);
                // Turn if ran into collision
                movement *= -1;
            }
            else
            {
                Debug.DrawRay(creaturePos, dir * collisionRange, Color.red);
            }
        }
        else
        {
            // Go to last position of target
            // Calculate which direction to go
            bool isTargetToRight = creaturePos.x >= lastPositionOfTarget.x;
            // Adjust input based off of cripple percentage and if slowed
            if (creature.GetCripplePercent(CreaturePartsType.Ground) <= .5f || slowed)
            {
                movement = isTargetToRight ? -Creature.WALK_INPUT : Creature.WALK_INPUT;
            }
            else
            {
                movement = isTargetToRight ? -Creature.RUN_INPUT : Creature.RUN_INPUT;
            }
        }
        creature.GroundMove(movement);
    }

    public void Exit() { }
}
