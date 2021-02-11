
using CreatureAttackLibrary;
using CreatureSystems;
using System.Collections.Generic;
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
            int attackId = (int)BipedalCreatureAttack.LOW_PUNCH;
            Dictionary<int, CreatureAttackFrame> frames = BipedalCreatureAttackLibrary.LOW_PUNCH_FRAMES;
            Damage dmg = new Damage(30, creature.Stats.AttackDamageType);
            attack = new CreatureAttack(attackId, frames, dmg);
        }
        return attack;
    }
}

public enum BipedalCreatureAttack
{
    LOW_PUNCH = 1,
}

public class CreatureAttack
{
    private int id;
    private Dictionary<int, CreatureAttackFrame> frames;
    private Damage damage;

    public CreatureAttack(int id, Dictionary<int, CreatureAttackFrame> frames, Damage damage)
    {
        this.id = id;
        this.frames = frames;
        this.damage = damage;
    }

    public int ID
    {
        get { return id; }
    }

    public Dictionary<int, CreatureAttackFrame> Frames
    {
        get { return frames; }
    }

    public Damage Damage
    {
        get { return damage; }
    }
}
