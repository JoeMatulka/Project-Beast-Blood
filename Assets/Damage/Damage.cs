using System;
using System.Collections.Generic;
using UnityEngine;

public enum DamageElementType
{
    FIRE, POISON, RAW
}

public enum DamageModType
{
    // Adds more KO damage
    STRIKE,
    // Adds more Trip damage
    CHOP,
    // Adds more Part damage
    SHARP,
}

public class Damage
{
    private Guid id;

    private readonly float value;

    private readonly DamageElementType element;

    // Modifier amount to specific types of damage
    private readonly Dictionary<DamageModType, float> mods = new Dictionary<DamageModType, float>()
    {
        { DamageModType.STRIKE, 1f}, { DamageModType.CHOP, 1f}, { DamageModType.SHARP, 1f},
    };

    private readonly Vector2 force;

    public Vector3 Position;

    public Damage(float value, DamageElementType element)
    {
        this.id = Guid.NewGuid();
        this.value = value;
        this.element = element;
    }

    public Damage(float value, DamageElementType element, Dictionary<DamageModType, float> mods)
    {
        this.id = Guid.NewGuid();
        this.value = value;
        this.element = element;
        this.mods = mods;
    }

    public Damage(float value, DamageElementType element, Vector2 force)
    {
        this.id = Guid.NewGuid();
        this.value = value;
        this.element = element;
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

    public Dictionary<DamageModType, float> Mods
    {
        get { return this.mods; }
    }

    public DamageElementType Type
    {
        get { return this.element; }
    }

    public Vector2 Force
    {
        get { return this.force; }
    }
}
