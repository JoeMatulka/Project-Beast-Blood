using CreatureSystems;
using UnityEngine;

public class CreatureGroundPursueBehvior : ICreatureState
{
    private const float WALK_INPUT = .5f;
    private const float RUN_INPUT = 1.5f;

    private readonly Creature creature;

    private readonly Transform target;

    private readonly float walkRange;
    private readonly float runRange;
    private readonly float collisionRange;

    private readonly LayerMask jumpLayerMask;

    public CreatureGroundPursueBehvior(Creature creature, Transform target, float walkRange, float runRange, float collisionRange)
    {
        this.creature = creature;
        this.target = target;
        this.walkRange = walkRange;
        this.runRange = runRange;
        this.collisionRange = collisionRange;
        this.jumpLayerMask = LayerMask.GetMask("Creature Jump Trigger");
    }

    public void Enter()
    {
        creature.GroundMove(0, false);
    }

    public void Execute()
    {
        bool jump = false;

        Vector2 creaturePos = creature.transform.localPosition;
        float distToTarget = Vector2.Distance(target.position, creaturePos);
        // Move towards target to get into attack range
        float input = distToTarget >= runRange ? RUN_INPUT : WALK_INPUT;
        // Adjust input based off of cripple percentage
        if (creature.GetCripplePercent(CreaturePartsType.Ground) <= .5f) input = WALK_INPUT;

        float movement = creaturePos.x > target.position.x ? -input : input;

        Vector2 dir = creature.IsFacingRight ? Vector2.right : Vector2.left;
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

        creature.GroundMove(movement, jump);
    }

    public void Exit()
    {
        creature.GroundMove(0, false);
    }
}
