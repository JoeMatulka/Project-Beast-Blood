using CreatureAttackLibrary;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttackFrame
{
    private CreatureAttackSpriteSwap[] spriteSwaps;
    private string[] activeHitboxes;
    // Used for the id of the effect
    private CreatureEffectID effectId;
    // Used as an ID look up on the creature to retrieve the transform where the effect will spawn
    private string effectSourceId;
    private float forwardMovement = 0f;

    public CreatureAttackFrame(string[] activeHitboxes, CreatureAttackSpriteSwap[] spriteSwaps, float forwardMovement)
    {
        this.activeHitboxes = activeHitboxes;
        this.spriteSwaps = spriteSwaps;
        this.forwardMovement = forwardMovement;
    }

    public CreatureAttackFrame(string[] activeHitboxes, CreatureEffectID effectId, string effectSourceId)
    {
        this.activeHitboxes = activeHitboxes;
        this.effectId = effectId;
        this.effectSourceId = effectSourceId;
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

    public string EffectSourceId
    {
        get { return this.effectSourceId; }
    }
}
