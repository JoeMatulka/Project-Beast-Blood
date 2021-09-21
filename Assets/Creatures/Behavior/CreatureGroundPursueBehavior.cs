using CreatureSystems;
using UnityEngine;

public struct CreatureGroundPursueBehvior : ICreatureState
{
    private readonly Creature creature;

    private readonly Transform target;

    private readonly float walkRange;
    private readonly float runRange;

    private readonly float sightRange;

    private readonly bool slowed;

    public CreatureGroundPursueBehvior(Creature creature, Transform target, float walkRange, float runRange, float sightRange, bool slowed = false)
    {
        this.creature = creature;
        this.target = target;
        this.walkRange = walkRange;
        this.runRange = runRange;
        this.sightRange = sightRange;
        this.slowed = slowed;
    }

    public void Enter() { }

    public void Execute()
    {
        Vector2 targetPos = target.position;
        Vector2 creaturePos = creature.transform.localPosition;

        if (IsTargetInLineOfSight(targetPos, creaturePos))
        {
            Pursue(targetPos, creaturePos);
        }
        else
        {
            // Lost sight of target, captures last position when target Line Of Sights Creature.
            // May not be best solution, definitely MVP solution
            creature.LastKnownTargetPos = targetPos;
            creature.Target = null;
        }
    }

    private bool IsTargetInLineOfSight(Vector2 targetPos, Vector2 creaturePos)
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(creaturePos, targetPos);
        Debug.DrawRay(creaturePos, targetPos - creaturePos, Color.green);
        bool los = true;
        foreach (RaycastHit2D hit in hits) {
            if (hit.transform.GetInstanceID().Equals(creature.transform.GetInstanceID()) || hit.transform.GetInstanceID().Equals(target.transform.GetInstanceID())) {
                // Skip collisions with self or target
                continue;
            }
            if (hit.collider != null)
            {
                // Target is not in line of sight
                los = false;
                break;
            }
        }
        return los;
    }

    private void Pursue(Vector2 targetPos, Vector2 creaturePos)
    {
        float distToTarget = Vector2.Distance(target.position, creaturePos);
        // Move towards target to get into attack range
        float input = distToTarget >= runRange ? Creature.RUN_INPUT : Creature.WALK_INPUT;
        // Adjust input based off of cripple percentage and if slowed
        if (creature.GetCripplePercent(CreaturePartsType.Ground) <= .5f || slowed) input = Creature.WALK_INPUT;

        float movement = creaturePos.x > target.position.x ? -input : input;

        creature.GroundMove(movement);
    }

    public void Exit() { }
}
