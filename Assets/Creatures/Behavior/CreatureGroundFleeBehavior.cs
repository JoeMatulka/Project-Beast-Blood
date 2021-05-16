using CreatureAttackLibrary;
using CreatureSystems;
using UnityEngine;

public class CreatureGroundFleeBehavior : ICreatureState
{
    private const float WALK_INPUT = .5f;
    private const float RUN_INPUT = 1.5f;

    private readonly Creature creature;

    private readonly float collisionRange;

    private readonly Vector2 fleeFrom;

    private readonly CreatureAttack roar;

    private readonly LayerMask groundLayerMask;

    public CreatureGroundFleeBehavior(Creature creature, float collisionRange, Vector2 fleeFrom)
    {
        this.creature = creature;
        this.collisionRange = collisionRange;
        this.fleeFrom = fleeFrom;
        this.groundLayerMask = LayerMask.GetMask("Ground");
    }

    public CreatureGroundFleeBehavior(Creature creature, float collisionRange, Vector2 fleeFrom, CreatureAttack roar)
    {
        this.creature = creature;
        this.collisionRange = collisionRange;
        this.fleeFrom = fleeFrom;
        this.roar = roar;
        this.groundLayerMask = LayerMask.GetMask("Ground");
    }

    public void Enter()
    {
        creature.IsFleeing = true;
        // Forget target since the creature is fleeing
        creature.Target = null;
        if (roar != null)
        {
            creature.Attack(roar);
        }
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
            creature.TimeSinceLastFlee = Time.time;
        }
    }

    private bool CheckCanFlee(Creature creature)
    {
        Vector2 creaturePos = creature.transform.localPosition;
        bool isFacingRight = creature.IsFacingRight;
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(creaturePos, dir, collisionRange, groundLayerMask);
        if (hit.collider != null)
        {
            Debug.DrawRay(creaturePos, dir * collisionRange, Color.green);
            return false;
        }
        else
        {
            Debug.DrawRay(creaturePos, dir * collisionRange, Color.red);
            return true;
        }
    }

    public void Exit() { }
}
