
using CreatureAttackLibrary;
using CreatureSystems;
using System.Collections.Generic;
using UnityEngine;
/**
* Class responsible for determining the type of attack used by a creature
*/
public static class CreatureAttackBehavior
{
    public static CreatureAttack CalculateAttack(Vector2 targetPos, in Creature creature)
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

    private static CreatureAttack GetBipedalCreatureAttack(Vector2 targetPos, in Creature creature)
    {
        CreatureAttack attack = null;
        Vector2 creaturePos = creature.transform.localPosition;
        if (((creature.IsFacingRight && targetPos.x > creaturePos.x) || (!creature.IsFacingRight && targetPos.x < creaturePos.x)) && targetPos.y <= creaturePos.y)
        {
            // Creature is currently facing target and target is lower than creature
            int attackId = (int)BipedalCreatureAttack.LOW_PUNCH;
            Dictionary<int, CreatureAttackFrame> frames = BipedalCreatureAttackLibrary.LOW_PUNCH_FRAMES;
            // TODO Place holder for now, damage should come off of creature for type of damage and how hard it hits
            Damage dmg = new Damage(30, DamageType.RAW);
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
