using CreatureAttackLibrary;
using System.Collections.Generic;

public class CreatureAttackFrame
{
    private CreatureAttackSpriteSwap[] spriteSwaps;
    private string[] activeHitboxes;
    private float forwardMovement = 0f;

    public CreatureAttackFrame(string[] activeHitboxes, CreatureAttackSpriteSwap[] spriteSwaps, float forwardMovement)
    {
        this.activeHitboxes = activeHitboxes;
        this.spriteSwaps = spriteSwaps;
        this.forwardMovement = forwardMovement;
    }

    public CreatureAttackFrame(string[] activeHitboxes) {
        this.activeHitboxes = activeHitboxes;
    }

    public CreatureAttackFrame(string[] activeHitboxes, CreatureAttackSpriteSwap[] spriteSwaps)
    {
        this.activeHitboxes = activeHitboxes;
        this.spriteSwaps = spriteSwaps;
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
}
