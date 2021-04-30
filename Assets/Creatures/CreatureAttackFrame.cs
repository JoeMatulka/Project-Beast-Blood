using CreatureAttackLibrary;
using System.Collections.Generic;

public class CreatureAttackFrame
{
    private CreatureAttackSpriteSwap[] spriteSwaps;
    private string[] activeHitboxes;
    private float forwardMovement;

    public CreatureAttackFrame(string[] activeHitboxes, CreatureAttackSpriteSwap[] spriteSwaps = null, float forwardMovement = 0f)
    {
        this.activeHitboxes = activeHitboxes;
        this.spriteSwaps = spriteSwaps;
        this.forwardMovement = forwardMovement;
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
