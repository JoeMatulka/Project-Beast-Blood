using HitboxSystem;

public class CreatureAttackFrame
{
    // TODO Build this class out to allow to swap sprites on creature parts
    private string[] activeHitboxes;
    private float forwardMovement;

    public CreatureAttackFrame(string[] activeHitboxes, float forwardMovement)
    {
        this.activeHitboxes = activeHitboxes;
        this.forwardMovement = forwardMovement;
    }

    public string[] ActiveHitboxes
    {
        get { return this.activeHitboxes; }
    }

    public float ForwardMovement
    {
        get { return this.forwardMovement; }
    }
}
