using CreatureSystems;
using UnityEngine;

public class CreatureGroundFollow : ICreatureState
{
    protected const float WALK_INPUT = 1f;
    protected const float RUN_INPUT = 2f;

    private readonly Creature creature;

    private readonly Transform target;

    private readonly float walkRange;
    private readonly float runRange;

    public CreatureGroundFollow(Creature creature, Transform target, float walkRange, float runRange)
    {
        this.creature = creature;
        this.target = target;
        this.walkRange = walkRange;
        this.runRange = runRange;
    }

    public void Enter() { }

    public void Execute()
    {
        float movement;
        bool jump = false;

        float distToTarget = Vector2.Distance(target.position, creature.transform.position);
        // Move towards target to get into attack range
        float input = distToTarget >= (runRange * walkRange) ? RUN_INPUT : WALK_INPUT;
        // Adjust input based off of cripple percentage
        if (creature.GetCripplePercent(CreaturePartsType.Ground) <= .5f) input = WALK_INPUT;

        movement = creature.transform.position.x > target.position.x ? -input : input;

        creature.GroundMove(movement, jump);
    }

    public void Exit() { }
}
