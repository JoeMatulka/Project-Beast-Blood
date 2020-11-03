﻿
using UnityEngine;
/**
* Class responsible for determining the type of attack used by a creature
*/
public static class CreatureAttackBehavior
{
    public static int GetAttack(Vector2 targetPos, Creature creature)
    {
        int attack = 1;
        switch (creature.Type)
        {
            case CreatureType.Bipedal:
                attack = GetBipedalCreatureAttack(targetPos, creature);
                break;
            default:
                break;
        }
        return attack;
    }

    private static int GetBipedalCreatureAttack(Vector2 targetPos, Creature creature)
    {
        int attack = 1;
        Vector2 creaturePos = creature.transform.localPosition;
        if(((creature.IsFacingRight && targetPos.x < creaturePos.x) || (!creature.IsFacingRight && targetPos.x > creaturePos.x)) && targetPos.y <= creaturePos.y) {
            // Creature is currently facing target and target is lower than creature
            attack = (int) BipedalCreatureAttack.LOW_PUNCH;
        }
        return attack;
    }
}

public enum BipedalCreatureAttack
{
    LOW_PUNCH = 1,
}
