
using CreatureAttackLibrary;
using CreatureSystems;
using UnityEngine;
/**
* Class responsible for determining the type of attack used by a creature
*/
public class CreatureAttackBehavior : ICreatureState
{
    private readonly Creature creature;

    private readonly Transform target;

    public CreatureAttackBehavior(Creature creature, Transform target)
    {
        this.creature = creature;
        this.target = target;
    }

    public void Enter() {
        // Halt movements
        creature.GroundMove(0, false);
    }

    public void Execute()
    {
        creature.Attack(CalculateAttack(target.position, creature));
    }

    public void Exit() { }

    private CreatureAttack CalculateAttack(Vector2 targetPos, in Creature creature)
    {
        CreatureAttack attack = null;
        foreach (CreatureAttack atk in creature.AttackSet)
        {
            if (atk.AttackCondition(targetPos, creature, atk.AttackPart))
            {
                attack = atk;
            }
        }
        return attack;
    }
}
