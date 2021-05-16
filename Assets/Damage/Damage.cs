using System;
using UnityEngine;

public enum DamageType
{
    FIRE, POISON, RAW
}

public class Damage
{
    private Guid id;

    private readonly float value;

    private readonly DamageType type;

    private readonly Vector2 force;

    public Damage(float value, DamageType type)
    {
        this.id = Guid.NewGuid();
        this.value = value;
        this.type = type;
    }

    public Damage(float value, DamageType type, Vector2 force)
    {
        this.id = Guid.NewGuid();
        this.value = value;
        this.type = type;
        this.force = force;
    }

    // Not the best thing, but used to reassign IDs to static damage objects (like on creature attacks)
    public void GenerateNewGuid()
    {
        this.id = Guid.NewGuid();
    }
    public Guid ID
    {
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

    public Vector2 Force
    {
        get { return this.force; }
    }
}
