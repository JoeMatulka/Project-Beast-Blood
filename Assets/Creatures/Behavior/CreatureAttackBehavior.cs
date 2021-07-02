
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

    private readonly float aggression;

    private const float MAX_AGGRESSION = 10;

    public CreatureAttackBehavior(Creature creature, Transform target, float aggression)
    {
        this.creature = creature;
        this.target = target;
        this.aggression = aggression;
    }

    public void Enter() { }

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
            if (CalculateAttackProbability(aggression) && atk.AttackCondition(targetPos, creature, atk.AttackPart))
            {
                attack = atk;
            }
        }
        return attack;
    }

    private bool CalculateAttackProbability(float aggression)
    {
        // Ensure no agressions passed to calculation are greater than the maximum agression level
        float a = aggression > MAX_AGGRESSION ? MAX_AGGRESSION : aggression;
        return Random.Range(a, MAX_AGGRESSION) <= a;
    }
}
