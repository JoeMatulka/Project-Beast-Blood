using CreatureAttackLibrary;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttackFrame
{
    private CreatureAttackSpriteSwap[] spriteSwaps;
    private string[] activeHitboxes;
    private CreatureEffectID effectId;
    private float forwardMovement = 0f;

    public CreatureAttackFrame(string[] activeHitboxes, CreatureAttackSpriteSwap[] spriteSwaps, float forwardMovement)
    {
        this.activeHitboxes = activeHitboxes;
        this.spriteSwaps = spriteSwaps;
        this.forwardMovement = forwardMovement;
    }

    public CreatureAttackFrame(string[] activeHitboxes, CreatureEffectID effectId)
    {
        this.activeHitboxes = activeHitboxes;
        this.effectId = effectId;
    }

    public CreatureAttackFrame(string[] activeHitboxes, CreatureAttackSpriteSwap[] spriteSwaps)
    {
        this.activeHitboxes = activeHitboxes;
        this.spriteSwaps = spriteSwaps;
    }

    public CreatureAttackFrame(string[] activeHitboxes)
    {
        this.activeHitboxes = activeHitboxes;
    }


    public string[] ActiveHitboxes
    {
        get { return this.activeHitboxes; }
    }

    public CreatureAttackSpriteSwap[] SpriteSwaps
    {
        get { return this.spriteSwaps; }
    }

    public float ForwardMovement
    {
        get { return this.forwardMovement; }
    }

    public CreatureEffectID EffectId
    {
        get { return this.effectId; }
    }
}
