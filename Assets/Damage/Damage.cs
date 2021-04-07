using System;

public enum DamageType
{
    FIRE, POISON, RAW
}

public class Damage
{
    private Guid id;

    private readonly float value;

    private readonly DamageType type;

    public Damage(float value, DamageType type)
    {
        this.id = Guid.NewGuid();
        this.value = value;
        this.type = type;
    }

    // Not the best thing, but used to reassign IDs to static damage objects (like on creature attacks)
    public void GenerateNewGuid() {
        this.id = Guid.NewGuid();
    }
    public Guid ID {
        get { return this.id; }
    }

    public float Value
    {
        get { return this.value; }
    }

    public DamageType Type
    {
        get { return this.type; }
    }
}
