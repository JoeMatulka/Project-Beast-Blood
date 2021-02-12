
using CreatureAttackLibrary;
using CreatureSystems;
using UnityEngine;
/**
* Class responsible for determining the type of attack used by a creature
*/
public class CreatureAttackBehavior: ICreatureState
{
    private readonly Creature creature;

    private readonly Transform target;

    public CreatureAttackBehavior(Creature creature, Transform target)
    {
        this.creature = creature;
        this.target = target;
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
        switch (creature.Stats.CreatureType)
        {
            case CreatureType.Bipedal:
                attack = GetBipedalCreatureAttack(targetPos, creature);
                break;
            default:
                break;
        }
        return attack;
    }

    private CreatureAttack GetBipedalCreatureAttack(Vector2 targetPos, in Creature creature)
    {
        CreatureAttack attack = null;
        Vector2 creaturePos = creature.transform.localPosition;
        if (((creature.IsFacingRight && targetPos.x > creaturePos.x) || (!creature.IsFacingRight && targetPos.x < creaturePos.x)) && targetPos.y <= creaturePos.y)
        {
            // Creature is currently facing target and target is lower than creature
            attack = BaseBipedalCreatureAttackLibrary.LOW_PUNCH;
            Damage dmg = new Damage(30, creature.Stats.AttackDamageType);
            attack.Damage = dmg;
        }
        return attack;
    }
}
