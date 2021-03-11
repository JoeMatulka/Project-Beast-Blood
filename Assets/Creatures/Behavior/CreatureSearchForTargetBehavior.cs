﻿using CreatureSystems;
using UnityEngine;

public class CreatureSearchForTargetBehavior : ICreatureState
{
    private const float WALK_INPUT = .5f;
    private const float RUN_INPUT = 1.5f;

    private readonly Creature creature;
    private Vector2 lastPositionOfTarget;
    private readonly float sightRange;
    private readonly float collisionRange;
    private readonly LayerMask sightLayerMask;
    private readonly LayerMask jumpLayerMask;

    public CreatureSearchForTargetBehavior(Creature creature, float sightRange, float collisionRange, LayerMask sightLayerMask)
    {
        this.creature = creature;
        this.sightRange = sightRange;
        this.collisionRange = collisionRange;
        this.sightLayerMask = sightLayerMask;
        this.jumpLayerMask = LayerMask.GetMask("Creature Jump Trigger");
    }

    public CreatureSearchForTargetBehavior(Creature creature, Vector2 lastPositionOfTarget, float sightRange, float collisionRange, LayerMask sightLayerMask)
    {
        this.creature = creature;
        this.lastPositionOfTarget = lastPositionOfTarget;
        this.sightRange = sightRange;
        this.collisionRange = collisionRange;
        this.sightLayerMask = sightLayerMask;
        this.jumpLayerMask = LayerMask.GetMask("Creature Jump Trigger");
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

        if (lastPositionOfTarget == Vector2.zero || Vector2.Distance(lastPositionOfTarget, creaturePos) <= collisionRange)
        {
            lastPositionOfTarget = Vector2.zero;
            // Seek aimlessly if no previous position of target has been set
            bool isFacingRight = creature.IsFacingRight;
            movement = isFacingRight ? WALK_INPUT : -WALK_INPUT;
            Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(creaturePos, dir, collisionRange, LayerMask.GetMask("Level"));
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
            movement = isTargetToRight ? -RUN_INPUT : RUN_INPUT;
            Vector2 dir = isTargetToRight ? Vector2.left : Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(creaturePos, dir, collisionRange, jumpLayerMask);
            if (hit.collider != null)
            {
                Debug.DrawRay(creaturePos, dir * collisionRange, Color.green);
                // Jump if collisions are in the way
                jump = true;
            }
            else
            {
                Debug.DrawRay(creaturePos, dir * collisionRange, Color.red);
            }
        }

        creature.GroundMove(movement, jump);
    }

    public void Exit()
    {

    }
}
