using CreatureSystems;
using UnityEngine;

public class CreatureGroundPursueBehvior : ICreatureState
{
    protected const float WALK_INPUT = .75f;
    protected const float RUN_INPUT = 1.5f;

    private readonly Creature creature;

    private readonly Transform target;

    private readonly float walkRange;
    private readonly float runRange;

    public CreatureGroundPursueBehvior(Creature creature, Transform target, float walkRange, float runRange)
    {
        this.creature = creature;
        this.target = target;
        this.walkRange = walkRange;
        this.runRange = runRange;
    }

    public void Enter()
    {
        creature.GroundMove(0, false);
    }

    public void Execute()
    {
        float movement;
        bool jump = false;

        float distToTarget = Vector2.Distance(target.position, creature.transform.position);
        // Move towards target to get into attack range
        float input = distToTarget >= runRange ? RUN_INPUT : WALK_INPUT;
        // Adjust input based off of cripple percentage
        if (creature.GetCripplePercent(CreaturePartsType.Ground) <= .5f) input = WALK_INPUT;

        movement = creature.transform.position.x > target.position.x ? -input : input;
        creature.GroundMove(movement, jump);
    }

    public void Exit()
    {
        creature.GroundMove(0, false);
    }
}
