using CreatureSystems;
using UnityEngine;

public struct CreatureGroundPursueBehvior : ICreatureState
{
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

    public void Enter() { }

    public void Execute()
    {
        Vector2 creaturePos = creature.transform.localPosition;
        float distToTarget = Vector2.Distance(target.position, creaturePos);
        // Move towards target to get into attack range
        float input = distToTarget >= runRange ? Creature.RUN_INPUT : Creature.WALK_INPUT;
        // Adjust input based off of cripple percentage
        if (creature.GetCripplePercent(CreaturePartsType.Ground) <= .5f) input = Creature.WALK_INPUT;

        float movement = creaturePos.x > target.position.x ? -input : input;

        creature.GroundMove(movement);
    }

    public void Exit() { }
}
