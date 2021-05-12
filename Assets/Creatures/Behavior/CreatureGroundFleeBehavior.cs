using CreatureSystems;
using UnityEngine;

public class CreatureGroundFleeBehavior : ICreatureState
{
    private const float WALK_INPUT = .5f;
    private const float RUN_INPUT = 1.5f;

    private readonly Creature creature;

    private readonly float collisionRange;

    private readonly Vector2 fleeFrom;

    public CreatureGroundFleeBehavior(Creature creature, float collisionRange, Vector2 fleeFrom)
    {
        this.creature = creature;
        this.collisionRange = collisionRange;
        this.fleeFrom = fleeFrom;
    }

    public void Enter()
    {
        creature.IsFleeing = true;
        // Forgot target since the creature is fleeing
        creature.Target = null;
    }

    public void Execute()
    {
        if (CheckCanFlee(creature))
        {
            float input = RUN_INPUT;
            Vector2 creaturePos = creature.transform.localPosition;
            // Adjust input based off of cripple percentage
            if (creature.GetCripplePercent(CreaturePartsType.Ground) <= .5f) input = WALK_INPUT;
            float movement = creaturePos.x > fleeFrom.x ? input : -input;

            creature.GroundMove(movement);
        }
        else
        {
            creature.IsFleeing = false;
        }
    }

    private bool CheckCanFlee(Creature creature)
    {
        return true;
    }

    public void Exit() {
        creature.TimeSinceLastFlee = Time.time;
    }
}
