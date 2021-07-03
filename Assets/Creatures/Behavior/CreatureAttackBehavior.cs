
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

    private const float ATK_TIME_BUFFER = 1.5f;
    private const float MAX_ATK_TIME_BUFFEER = 5f;
    private float lastTimeAttemptToAttack = 0f;
    private float timeSinceLastAttack = 0f;

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
                timeSinceLastAttack = Time.time;
            }
        }
        return attack;
    }

    private bool CalculateAttackProbability(float aggression)
    {
        if ((Time.time - timeSinceLastAttack) >= MAX_ATK_TIME_BUFFEER)
        {
            // If max time for buffer has passed since last attack, make sure the creature attacks to avoid long pauses
            return true;
        }
        // Ensure no agressions passed to calculation are greater than the maximum agression level
        float a = aggression >= MAX_AGGRESSION ? MAX_AGGRESSION : aggression;
        if (a != MAX_AGGRESSION && (Time.time - lastTimeAttemptToAttack) <= ATK_TIME_BUFFER)
        {
            //Buffer attacks if not at max aggression, this is because the update call happens every frame
            return false;
        }
        lastTimeAttemptToAttack = Time.time;
        return Random.Range(0, MAX_AGGRESSION) <= a;
    }
}
